using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using System.Xml;

namespace RomaneioService
{
    public class ServiceErrors
    {
        public string ErrorMessage { get; set; }
        public string ErrorMessageLimiteLargura { get; set; }
        public string ErrorMessageLimiteGramatura { get; set; }

        public string ErrorMessageLimiteTipo { get; set; }
        public string ErrorMessageLimitePeso { get; set; }
        public string ErrorMessageLimiteComprimento { get; set; }
        public string ErrorMessageTipo { get; set; }


        public ServiceErrors()
        {
            ErrorMessage = "Romaneio Cadastrado";

            ErrorMessageLimiteLargura = "Campo <NR_Largura> inválido [0,4 - 3,0]";

            ErrorMessageLimiteGramatura = "Campo <NR_Gramatura> inválido! [0 - 600]";

            ErrorMessageLimiteTipo = "Campo <OP_Tipo> inválido! [1 - 3]";

            ErrorMessageLimitePeso = "Campo <NR_Peso> inválido! [0,3 - 160]";

            ErrorMessageLimiteComprimento = "Campo <NR_Comprimento> inválido! [0 - 1200]";

            ErrorMessageTipo = "O campo <NR_Cnpj_Faccionista> não deve ter conteúdo, enquanto <OP_Tipo_Lote> igual a '1'";
        }


    }
}