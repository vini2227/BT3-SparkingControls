using System;
using System.IO;
using System.Collections.Generic;

static class UiLanguage {
    public static string Code="en";
    public static readonly string[] Codes={"en","fr","it","de","es","ar","id","es-419","pt","pl","ru","zh-Hans","zh-Hant","ja","ko","th"};
    public static readonly string[] NativeNames={"English","Français","Italiano","Deutsch","Español (España)","العربية","Bahasa Indonesia","Español (Latinoamérica)","Português (Brasil)","Polski","Русский","简体中文","繁體中文","日本語","한국어","ไทย"};
    public static int CurrentIndex {get {int index=Array.IndexOf(Codes,Code);return index<0?0:index;}}
    public static bool IsRightToLeft {get {return Code=="ar";}}
    sealed class Catalog {public Dictionary<string,Dictionary<string,string>> translations=new Dictionary<string,Dictionary<string,string>>();}
    static Catalog catalog;static bool catalogLoaded;
    static void LoadCatalog(){
        if(catalogLoaded)return;catalogLoaded=true;
        try {catalog=new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Catalog>(File.ReadAllText(Path.Combine(Program.Root,"assets","ui-translations.json")));}
        catch(IOException){}catch(UnauthorizedAccessException){}catch(ArgumentException){}catch(InvalidOperationException){}
    }

    static readonly Dictionary<string,string[]> Texts=new Dictionary<string,string[]>(StringComparer.Ordinal) {
        { "Local do BT3 Decomp", new[]{"BT3 Decomp location","Ubicación de BT3 Decomp","Local do BT3 Decomp"} },
        { "Idioma", new[]{"Language","Idioma","Idioma"} },
        { "Procurar...", new[]{"Browse...","Examinar...","Procurar..."} },
        { "Selecione o Tenkaichi3Decomp.exe. O caminho fica salvo para as próximas aberturas.", new[]{"Select Tenkaichi3Decomp.exe. Its location will be saved for future launches.","Selecciona Tenkaichi3Decomp.exe. La ubicación se guardará para las próximas aperturas.","Selecione o Tenkaichi3Decomp.exe. O caminho fica salvo para as próximas aberturas."} },
        { "Caminho salvo carregado.", new[]{"Saved location loaded.","Ubicación guardada cargada.","Caminho salvo carregado."} },
        { "Escolha o executável na pasta atual do jogo.", new[]{"Choose the executable in the current game folder.","Elige el ejecutable en la carpeta actual del juego.","Escolha o executável na pasta atual do jogo."} },
        { "Salvar e iniciar", new[]{"Save and start","Guardar e iniciar","Salvar e iniciar"} },
        { "Salvar caminho", new[]{"Save location","Guardar ubicación","Salvar caminho"} },
        { "Fechar", new[]{"Close","Cerrar","Fechar"} },
        { "Clique em Salvar caminho ou Salvar e iniciar para confirmar.", new[]{"Click Save location or Save and start to confirm.","Pulsa Guardar ubicación o Guardar e iniciar para confirmar.","Clique em Salvar caminho ou Salvar e iniciar para confirmar."} },
        { "Escolha o executável do BT3 Decomp", new[]{"Choose the BT3 Decomp executable","Elige el ejecutable de BT3 Decomp","Escolha o executável do BT3 Decomp"} },
        { "Executáveis (*.exe)", new[]{"Executables (*.exe)","Ejecutables (*.exe)","Executáveis (*.exe)"} },
        { "Selecione o arquivo executável (.exe) do jogo.", new[]{"Select the game executable (.exe).","Selecciona el ejecutable (.exe) del juego.","Selecione o arquivo executável (.exe) do jogo."} },
        { "Caminho verificado e salvo.", new[]{"Location verified and saved.","Ubicación verificada y guardada.","Caminho verificado e salvo."} },
        { "Não foi possível salvar. Confira o caminho e tente novamente.", new[]{"Could not save. Check the location and try again.","No se pudo guardar. Revisa la ubicación e inténtalo de nuevo.","Não foi possível salvar. Confira o caminho e tente novamente."} },
        { "Verificar caminho", new[]{"Check location","Verificar ubicación","Verificar caminho"} },
        { "Não foi possível salvar o idioma.", new[]{"Could not save the language.","No se pudo guardar el idioma.","Não foi possível salvar o idioma."} },
        { "FUSÕES", new[]{"FUSIONS","FUSIONES","FUSÕES"} },
        { "FORMAS", new[]{"FORMS","FORMAS","FORMAS"} },
        { "TRANSFORMAÇÕES", new[]{"TRANSFORMATIONS","TRANSFORMACIONES","TRANSFORMAÇÕES"} },
        { "ATAQUES ESPECIAIS", new[]{"SUPER ATTACKS","ATAQUES ESPECIALES","ATAQUES ESPECIAIS"} },
        { "Leitura de memoria indisponivel em 0x", new[]{"Memory read unavailable at 0x","Lectura de memoria no disponible en 0x","Leitura de memoria indisponivel em 0x"} },
        { "Nao foi possivel escrever o patch em memoria.", new[]{"Could not write the patch to memory.","No se pudo escribir el parche en la memoria.","Nao foi possivel escrever o patch em memoria."} },
        { "Nao foi possivel proteger a memoria do complemento.", new[]{"Could not protect add-on memory.","No se pudo proteger la memoria del complemento.","Nao foi possivel proteger a memoria do complemento."} },
        { "Tabela de custo inesperada.", new[]{"Unexpected cost table.","Tabla de costes inesperada.","Tabela de custo inesperada."} },
        { "Nao foi encontrada uma regiao livre para o complemento. Nenhum arquivo foi alterado.", new[]{"No free memory region was found for the add-on. No files were changed.","No se encontró una región de memoria libre para el complemento. No se modificó ningún archivo.","Nao foi encontrada uma regiao livre para o complemento. Nenhum arquivo foi alterado."} },
        { "Perfil do complemento invalido.", new[]{"Invalid add-on profile.","Perfil del complemento no válido.","Perfil do complemento invalido."} },
        { "O executavel escolhido nao existe.", new[]{"The selected executable does not exist.","El ejecutable seleccionado no existe.","O executavel escolhido nao existe."} },
        { "O payload nao corresponde ao perfil. Extraia o pacote novamente.", new[]{"The payload does not match the profile. Extract the complete package again.","El payload no coincide con el perfil. Extrae de nuevo el paquete completo.","O payload nao corresponde ao perfil. Extraia o pacote novamente."} },
        { "Feche o jogo antes de iniciar pelo complemento. Ele aplica o patch antes do primeiro frame.", new[]{"Close the game before starting it through the add-on. The patch is applied before the first frame.","Cierra el juego antes de iniciarlo con el complemento. El parche se aplica antes del primer fotograma.","Feche o jogo antes de iniciar pelo complemento. Ele aplica o patch antes do primeiro frame."} },
        { "Nao foi possivel iniciar o jogo. Erro Windows ", new[]{"Could not start the game. Windows error ","No se pudo iniciar el juego. Error de Windows ","Nao foi possivel iniciar o jogo. Erro Windows "} },
        { "Nao foi possivel verificar a base do jogo.", new[]{"Could not verify the game image base.","No se pudo verificar la dirección base del juego.","Nao foi possivel verificar a base do jogo."} },
        { "Endereco base inesperado. O jogo foi encerrado antes de aplicar o patch.", new[]{"Unexpected image base. The game was closed before applying the patch.","Dirección base inesperada. El juego se cerró antes de aplicar el parche.","Endereco base inesperado. O jogo foi encerrado antes de aplicar o patch."} },
        { "As instrucoes em memoria diferem da versao verificada.", new[]{"The instructions in memory differ from the verified version.","Las instrucciones en memoria difieren de la versión verificada.","As instrucoes em memoria diferem da versao verificada."} },
        { "Falha ao preparar o patch em memoria.", new[]{"Could not prepare the memory patch.","No se pudo preparar el parche en memoria.","Falha ao preparar o patch em memoria."} },
        { "Falha ao restaurar protecao da memoria.", new[]{"Could not restore memory protection.","No se pudo restaurar la protección de memoria.","Falha ao restaurar protecao da memoria."} },
        { "Falha ao sincronizar as instrucoes.", new[]{"Could not synchronize instructions.","No se pudieron sincronizar las instrucciones.","Falha ao sincronizar as instrucoes."} },
        { "Falha na releitura do patch.", new[]{"Patch verification failed.","Falló la verificación del parche.","Falha na releitura do patch."} },
        { "Falha ao liberar o inicio do jogo.", new[]{"Could not resume game startup.","No se pudo continuar el inicio del juego.","Falha ao liberar o inicio do jogo."} },
        { "Executavel PE truncado ou invalido.", new[]{"Truncated or invalid PE executable.","Ejecutable PE truncado o no válido.","Executavel PE truncado ou invalido."} },
        { "O arquivo escolhido nao e um executavel Windows valido.", new[]{"The selected file is not a valid Windows executable.","El archivo seleccionado no es un ejecutable de Windows válido.","O arquivo escolhido nao e um executavel Windows valido."} },
        { "Esta revisao exige o port Windows x64.", new[]{"This revision requires the Windows x64 port.","Esta revisión requiere el port de Windows x64.","Esta revisao exige o port Windows x64."} },
        { "Formato PE64 inesperado.", new[]{"Unexpected PE64 format.","Formato PE64 inesperado.","Formato PE64 inesperado."} },
        { "Layout de memoria do port fora da faixa suportada.", new[]{"The port memory layout is outside the supported range.","La distribución de memoria del port está fuera del rango compatible.","Layout de memoria do port fora da faixa suportada."} },
        { "Tabela de secoes PE invalida.", new[]{"Invalid PE section table.","Tabla de secciones PE no válida.","Tabela de secoes PE invalida."} },
        { "Secao PE fora dos limites da imagem.", new[]{"PE section outside image bounds.","Sección PE fuera de los límites de la imagen.","Secao PE fora dos limites da imagem."} },
        { "Esta compilacao removeu a tabela de simbolos. O auxiliar nao consegue reconhecer esta versao automaticamente; nenhum patch foi aplicado.", new[]{"This build has no symbol table. The add-on cannot recognize this version automatically; no patch was applied.","Esta compilación no tiene tabla de símbolos. El complemento no puede reconocer esta versión automáticamente; no se aplicó ningún parche.","Esta compilacao removeu a tabela de simbolos. O auxiliar nao consegue reconhecer esta versao automaticamente; nenhum patch foi aplicado."} },
        { "Tabela de simbolos fora dos limites.", new[]{"Symbol table outside valid bounds.","Tabla de símbolos fuera de los límites válidos.","Tabela de simbolos fora dos limites."} },
        { "Tabela de nomes PE invalida.", new[]{"Invalid PE string table.","Tabla de nombres PE no válida.","Tabela de nomes PE invalida."} },
        { "Nome de simbolo PE fora dos limites.", new[]{"PE symbol name outside valid bounds.","Nombre de símbolo PE fuera de los límites válidos.","Nome de simbolo PE fora dos limites."} },
        { "Nome de simbolo PE invalido.", new[]{"Invalid PE symbol name.","Nombre de símbolo PE no válido.","Nome de simbolo PE invalido."} },
        { "Registro auxiliar PE invalido.", new[]{"Invalid auxiliary PE record.","Registro auxiliar PE no válido.","Registro auxiliar PE invalido."} },
        { "Atualizacao ainda nao reconhecida: falta o simbolo ", new[]{"Unsupported update: missing symbol ","Actualización aún no compatible: falta el símbolo ","Atualizacao ainda nao reconhecida: falta o simbolo "} },
        { ". Nenhum patch foi aplicado.", new[]{". No patch was applied.",". No se aplicó ningún parche.",". Nenhum patch foi aplicado."} },
        { "Trecho de codigo fora de uma secao legivel.", new[]{"Code lies outside a readable section.","El código está fuera de una sección legible.","Trecho de codigo fora de uma secao legivel."} },
        { "O simbolo ", new[]{"The symbol ","El símbolo ","O simbolo "} },
        { " nao aponta para uma funcao executavel.", new[]{" does not point to an executable function."," no apunta a una función ejecutable."," nao aponta para uma funcao executavel."} },
        { "Falta compatibility.json. Extraia o pacote atualizado inteiro.", new[]{"Missing compatibility.json. Extract the entire updated package.","Falta compatibility.json. Extrae el paquete actualizado completo.","Falta compatibility.json. Extraia o pacote atualizado inteiro."} },
        { "Metadados de compatibilidade nao correspondem ao complemento.", new[]{"Compatibility metadata does not match the add-on.","Los metadatos de compatibilidad no coinciden con el complemento.","Metadados de compatibilidade nao correspondem ao complemento."} },
        { "A atualizacao mudou a estrutura de ", new[]{"The update changed the structure of ","La actualización cambió la estructura de ","A atualizacao mudou a estrutura de "} },
        { ". Esta mudanca exige revisao do complemento; nenhum patch foi aplicado.", new[]{". The add-on needs an update for this change; no patch was applied.",". Es necesario revisar el complemento para este cambio; no se aplicó ningún parche.",". Esta mudanca exige revisao do complemento; nenhum patch foi aplicado."} },
        { "A atualizacao mudou as instrucoes de ", new[]{"The update changed instructions in ","La actualización cambió las instrucciones de ","A atualizacao mudou as instrucoes de "} },
        { "). Nao e apenas uma mudanca de endereco; nenhum patch foi aplicado.", new[]{"). This is more than an address change; no patch was applied.","). No es solo un cambio de dirección; no se aplicó ningún parche.","). Nao e apenas uma mudanca de endereco; nenhum patch foi aplicado."} },
        { "Referencia de payload invalida.", new[]{"Invalid payload reference.","Referencia de payload no válida.","Referencia de payload invalida."} },
        { "Ponto de patch adaptativo invalido.", new[]{"Invalid adaptive patch site.","Punto de parche adaptativo no válido.","Ponto de patch adaptativo invalido."} },
        { "A chamada de agarrão mudou de destino; nenhum patch foi aplicado.", new[]{"The grab call destination changed; no patch was applied.","La llamada de agarre cambió de destino; no se aplicó ningún parche.","A chamada de agarrão mudou de destino; nenhum patch foi aplicado."} },
        { "PREVIEW VISUAL — valores ilustrativos; no jogo, os dados vêm do personagem ativo.", new[]{"VISUAL PREVIEW — sample values; gameplay data comes from the active character.","VISTA PREVIA — valores ilustrativos; en partida se usan los datos del personaje activo.","PREVIEW VISUAL — valores ilustrativos; no jogo, os dados vêm do personagem ativo."} },
        { "Posição dos painéis", new[]{"Panel placement","Posición de los paneles","Posição dos painéis"} },
        { "Centralizada", new[]{"Centered","Centrada","Centralizada"} },
        { "Expandida", new[]{"Expanded","Expandida","Expandida"} },
        { "16:9 e ultrawide", new[]{"16:9 and ultrawide","16:9 y ultrawide","16:9 e ultrawide"} },
        { "Não foi possível salvar a posição dos painéis.", new[]{"Could not save the panel placement.","No se pudo guardar la posición de los paneles.","Não foi possível salvar a posição dos painéis."} },
    };
    public static string Text(string key) {
        LoadCatalog();Dictionary<string,string> language;string translated;
        if(catalog!=null&&catalog.translations!=null&&catalog.translations.TryGetValue(Code,out language)&&language.TryGetValue(key,out translated))return translated;
        string[] value;if(Texts.TryGetValue(key,out value))return value[Code=="es"||Code=="es-419"?1:Code=="pt"?2:0];return key;
    }
    public static void Load() {
        Code="en";
        try { string file=SavedSettings.FilePath("language.txt");if(File.Exists(file))Set(File.ReadAllText(file).Trim(),false); }catch(IOException){}catch(UnauthorizedAccessException){}
    }
    public static void Set(string code,bool persist) {
        string requested=(code??"en").Trim();
        if(requested.Equals("pt-BR",StringComparison.OrdinalIgnoreCase))requested="pt";
        if(requested.Equals("es-ES",StringComparison.OrdinalIgnoreCase))requested="es";
        if(requested.Equals("es-MX",StringComparison.OrdinalIgnoreCase))requested="es-419";
        if(requested.Equals("zh-CN",StringComparison.OrdinalIgnoreCase))requested="zh-Hans";
        if(requested.Equals("zh-TW",StringComparison.OrdinalIgnoreCase))requested="zh-Hant";
        code="en";foreach(string supported in Codes)if(supported.Equals(requested,StringComparison.OrdinalIgnoreCase)){code=supported;break;}

        if(persist)File.WriteAllText(SavedSettings.FilePath("language.txt"),code);
        Code=code;
    }
}
