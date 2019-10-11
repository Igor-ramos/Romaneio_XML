using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RomaneioService
{
    public class ContaCaractere
    {
        public string ContaCaracter { get; set; }



        public bool isOverLenght(int lenghtCharacter, int lenghtLimit)
        {
            return lenghtCharacter > lenghtLimit;
        }

        public string Max1(string contaCaracter, string Nome)
        {
            if (isOverLenght(contaCaracter.Length, 1))

            {

                return "Limite de Caracteres no campo '" + Nome + "' Excedido";
            }
            return null;
        }
        public string Max4(string contaCaracter, string Nome)
        {
            if (isOverLenght(contaCaracter.Length, 4))

            {

                return "Limite de Caracteres no campo '" + Nome + "' Excedido";
            }
            return null;
        }
        public string Max7(string contaCaracter, string Nome)
        {
            if (isOverLenght(contaCaracter.Length, 7))

            {

                return "Limite de Caracteres no campo '" + Nome + "' Excedido";
            }
            return null;
        }
        public string Max9(string contaCaracter, string Nome)
        {
            if (isOverLenght(contaCaracter.Length, 9))

            {

                return "Limite de Caracteres no campo '" + Nome + "' Excedido";
            }
            return null;
        }
        public string Max10(string contaCaracter, string Nome)
        {
            if (isOverLenght(contaCaracter.Length, 10))

            {

                return "Limite de Caracteres no campo '" + Nome + "' Excedido";
            }
            return null;
        }
        public string Max16(string contaCaracter, string Nome)
        {
            if (isOverLenght(contaCaracter.Length, 16))

            {
                return "Limite de Caracteres no campo '" + Nome + "' Excedido";
            }
            return null;
        }
        public string Max18(string contaCaracter, string Nome)
        {
            if (isOverLenght(contaCaracter.Length, 18))

            {

                return "Limite de Caracteres no campo '" + Nome + "' Excedido";
            }
            return null;
        }
        public string Max25(string contaCaracter, string Nome)
        {
            if (isOverLenght(contaCaracter.Length, 25))

            {
                return "Limite de Caracteres no campo '" + Nome + "' Excedido";
            }
            return null;
        }
        public string Max45(string contaCaracter, string Nome)
        {
            if (isOverLenght(contaCaracter.Length, 45))

            {
                return "Limite de Caracteres no campo '" + Nome + "' Excedido";
            }
            return null;
        }
        public string Max50(string contaCaracter, string Nome)
        {
            if (isOverLenght(contaCaracter.Length, 50))

            {
                return "Limite de Caracteres no campo '" + Nome + "' Excedido";
            }
            return null;
        }
        public string MaxVirgula(string contaCaracter)
        {

            if (contaCaracter.Length <= 4)
            {
                for (int i = 0; i < contaCaracter.Length; i++)
                {
                    if (contaCaracter.Length == 1)
                        return null;
                    if (i == 1)
                    {
                        if (contaCaracter[i] == ',')
                            return null;
                        else
                        {
                            return "formato de caracteres no campo '" + contaCaracter + "' esta errado";
                        }
                    }

                }
            }
            return "limite de caracteres no campo '" + contaCaracter + "' excedido";
        }
    }
}