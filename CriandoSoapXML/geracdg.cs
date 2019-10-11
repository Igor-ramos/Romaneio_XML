using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CriandoSoapXML
{
    public class geracdg
    {
        public string GerarCodigo()
        {
            long i = 1;
            foreach (byte b in Guid.NewGuid().ToByteArray())
            {
                i *= ((int)b + 1);
            }
            return string.Format("{0:x}", i - DateTime.Now.Ticks);
        }
    }
}