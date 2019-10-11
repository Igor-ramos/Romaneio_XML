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

            ErrorMessageLimiteLargura = "Largura Excedida";

            ErrorMessageLimiteGramatura = "Gramatura Excedida";

            ErrorMessageLimiteTipo = "Tipo Fora de Padrão";

            ErrorMessageLimitePeso = "Peso Excedido";

            ErrorMessageLimiteComprimento = "Comprimento Excedido";

            ErrorMessageTipo = "CNPJ deve ser '0' quando OP_Tipo_lote for igual a 1";
        }


    }
}