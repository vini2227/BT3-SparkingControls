using System;
using System.IO;
using System.Web.Script.Serialization;
static class VariantSupport {
    static readonly JavaScriptSerializer Json=new JavaScriptSerializer {MaxJsonLength=8*1024*1024};
    public static bool Resolve(PeImage image,Profile destination,out string report){
        report=null;string kind=image.HasSymbol("Port_CharaFormTarget")?"characters":image.HasSymbol("Port_QuarterFighter")?"tagteam":null;
        if(kind==null)return false;
        string root=Path.Combine(Program.Root,"variants",kind);
        var profile=Json.Deserialize<Profile>(File.ReadAllText(Path.Combine(root,"profile.json")));
        var metadata=Json.Deserialize<OnlineMetadata>(File.ReadAllText(Path.Combine(root,"compatibility.json")));
        // Commit the resolved profile only after every native signature has passed.
        report="Automatically detected: "+kind+Environment.NewLine+OnlineCompatibility.ResolveImage(image,profile,metadata,root);
        destination.format=profile.format;destination.game_sha256=profile.game_sha256;destination.payload_sha256=profile.payload_sha256;
        destination.image_base=profile.image_base;destination.allocation=profile.allocation;destination.allocation_size=profile.allocation_size;
        destination.patches=profile.patches;destination.symbols=profile.symbols;destination.relocations=profile.relocations;
        destination.resolved_payload=profile.resolved_payload;destination.variant=kind;
        return true;
    }
}

static class VariantTests {
    public static int Run(string exe){
        string log="";try{
            byte[] bytes=File.ReadAllBytes(exe);var image=new PeImage(bytes);var p=Program.LoadProfile();Program.Validate(exe,p);
            if(p.variant!="characters"&&p.variant!="tagteam")throw new Exception("Variant not detected");
            log="PASS automatic detection: "+p.variant+Environment.NewLine;
            var altered=(byte[])bytes.Clone();altered[image.PeOffset+8]^=0x5a;
            string report;var q=Program.LoadProfile();if(!VariantSupport.Resolve(new PeImage(altered),q,out report)||q.variant!=p.variant)throw new Exception("Timestamp update rejected");
            foreach(string name in new[]{"Port_PadRead","BtlInput_BuildCommands","Port_NetActive"}){
                var bad=(byte[])bytes.Clone();bad[image.RawOffset(image.Symbol(name),1)]^=1;bool refused=false;
                try{VariantSupport.Resolve(new PeImage(bad),Program.LoadProfile(),out report);}catch(IOException){refused=true;}
                if(!refused)throw new Exception("Changed contract accepted: "+name);
            }
            log+="PASS changed timestamp accepted; changed input, battle and network interfaces rejected."+Environment.NewLine;
            var metadata=new JavaScriptSerializer {MaxJsonLength=8*1024*1024}.Deserialize<OnlineMetadata>(File.ReadAllText(Path.Combine(Program.Root,"variants",p.variant,"compatibility.json")));
            var moved=(byte[])bytes.Clone();const int delta=0x10000;
            foreach(var section in image.Sections)Buffer.BlockCopy(BitConverter.GetBytes(section.Rva+delta),0,moved,section.Header+12,4);
            Buffer.BlockCopy(BitConverter.GetBytes(image.SizeOfImage+delta),0,moved,image.PeOffset+24+56,4);
            foreach(var f in metadata.functions)foreach(var reference in f.references)if(reference.kind=="absolute"){
                int off=image.RawOffset(image.Symbol(f.name)+reference.offset,reference.width);
                byte[] value=reference.width==8?BitConverter.GetBytes(BitConverter.ToInt64(moved,off)+delta):BitConverter.GetBytes(checked(BitConverter.ToInt32(moved,off)+delta));
                Buffer.BlockCopy(value,0,moved,off,value.Length);
            }
            var shifted=Program.LoadProfile();if(!VariantSupport.Resolve(new PeImage(moved),shifted,out report))throw new Exception("Relocated variant not recognized");
            if(shifted.patches[0].rva!=p.patches[0].rva+delta)throw new Exception("Hook did not relocate");
            foreach(var key in p.symbols.Keys)if(shifted.symbols[key]!=p.symbols[key]+delta)throw new Exception("HUD symbol did not relocate");
            log+="PASS moved variant sections, hook and HUD globals automatically resolved."+Environment.NewLine;

            var json=new JavaScriptSerializer {MaxJsonLength=8*1024*1024};
            File.WriteAllText(Path.Combine(Program.Root,"resolved-test-profile.json"),json.Serialize(p));
            File.WriteAllBytes(Path.Combine(Program.Root,"resolved-test.payload"),p.resolved_payload);
            File.WriteAllText(Path.Combine(Program.Root,"variant-test-results.txt"),log);return 0;
        }catch(Exception ex){File.WriteAllText(Path.Combine(Program.Root,"variant-test-results.txt"),log+ex);return 2;}
    }
}
