﻿using MySql.Data.MySqlClient;
using RomaneioService;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace CriandoSoapXML
{
    /// <summary>
    /// Descrição resumida de CriaXML
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // Para permitir que esse serviço da web seja chamado a partir do script, usando ASP.NET AJAX, remova os comentários da linha a seguir. 
    // [System.Web.Script.Services.ScriptService]
    public class CriaXML : System.Web.Services.WebService
    {        
        private string caminhoArquivo = Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);

        [WebMethod(Description = "Cadastra um Lote")]
        public xml CadastraLote(string CNPJ, string Url)
        {
            xml t = null;
            XmlDocument doc = new XmlDocument();
            geracdg geracdg = new geracdg();
            string codigo = geracdg.GerarCodigo();
            DateTime data = DateTime.Now.Date;

            //caminhoArquivo = Directory.GetParent(Directory.GetParent(caminhoArquivo).FullName).FullName;
            //caminhoArquivo += @"\filmes.xml";
            List<Erro> erroList = new List<Erro>();
            caminhoArquivo = Url;
            bool ArquivoDanificado;
            XmlTextReader xmlReader = new XmlTextReader(caminhoArquivo);
            try
            {
                doc.Load(caminhoArquivo);
            }
            catch (Exception ex)
            {
                ArquivoDanificado = true;
                Erro xmlInvalido = new Erro();

                xmlInvalido.codigo = "1000";
                xmlInvalido.tipo = "O XML é invalido";
                erroList.Add(xmlInvalido);

                xml dadosXML = new xml(erroList);
                return dadosXML;
            }

            XmlNodeList lote = doc.SelectNodes("lotes/Lote");
            XmlNodeList romaneio, peca;
            string vSqL = "SELECT ID_Cliente FROM Cadastro_Cliente WHERE Cnpj = '" + CNPJ + "'";
            string vSqL2 = "";
            string vSqL3 = "";
            int id_cliente;
            int ultimoid;
            MySqlCommand cme;
            string seleciona;
            var conection = new MySqlConnection("Server=seiren_dev.mysql.dbaas.com.br;Port=3306;Database=seiren_dev;Uid=seiren_dev;Pwd=S3iR3n@1973dev;");

            MySqlCommand cmd = new MySqlCommand(vSqL, conection);


            conection.Open();
            id_cliente = Convert.ToInt32(cmd.ExecuteScalar());
            string idcliente = id_cliente.ToString();
            conection.Close();



            conection.Open();

            MySqlTransaction Trans = conection.BeginTransaction();
            cmd = new MySqlCommand("START TRANSACTION;", conection);
            cmd.Transaction = Trans;

            List<Lote> lotes = new List<Lote>();
            try
            {
                      
                    
                foreach (XmlNode itemlote in lote)
                {
                    string NR_Nota_Fiscal = itemlote.SelectSingleNode("NR_Nota_Fiscal").InnerText;
                    if (NR_Nota_Fiscal == null)
                    {
                        Erro xmlInvalido = new Erro();

                        xmlInvalido.codigo = "1002";
                        xmlInvalido.tipo = "Campo <NR_Nota_Fiscal> vazio!";
                        erroList.Add(xmlInvalido);

                        xml dadosXML = new xml(erroList);
                        return dadosXML;
                    }
                    string DT_Emissao = itemlote.SelectSingleNode("DT_Emissao").InnerText;
                    if (DT_Emissao == null)
                    {
                        Erro xmlInvalido = new Erro();

                        xmlInvalido.codigo = "1004";
                        xmlInvalido.tipo = "Campo <DT_Emissao> vazio!";
                        erroList.Add(xmlInvalido);

                        xml dadosXML = new xml(erroList);
                        return dadosXML;
                    }
                    if (DT_Emissao[4] != '-')
                    {
                        Erro xmlInvalido = new Erro();

                        xmlInvalido.codigo = "1036";
                        xmlInvalido.tipo = "Campo <DT_Emissao> Tem formato de 'yyyy-mm-dd'";
                        erroList.Add(xmlInvalido);

                        xml dadosXML = new xml(erroList);
                        return dadosXML;
                    }
                    DateTime dataxml = Convert.ToDateTime(DT_Emissao);
                    if (dataxml > data)
                    {
                        Erro xmlInvalido = new Erro();

                        xmlInvalido.codigo = "1005";
                        xmlInvalido.tipo = "Campo <DT_Emissao> não pode conter uma data maior que a data atual! (yyyy-mm-dd)";
                        erroList.Add(xmlInvalido);

                        xml dadosXML = new xml(erroList);
                        return dadosXML;
                    }
                    string OP_Tipo_Lote = itemlote.SelectSingleNode("OP_Tipo_Lote").InnerText;
                    if (OP_Tipo_Lote == null)
                    {
                        Erro xmlInvalido = new Erro();

                        xmlInvalido.codigo = "1006";
                        xmlInvalido.tipo = "Campo <OP_Tipo_Lote> vazio!";
                        erroList.Add(xmlInvalido);

                        xml dadosXML = new xml(erroList);
                        return dadosXML;
                    }
                    string NR_Cnpj_Faccionista = itemlote.SelectSingleNode("NR_Cnpj_Faccionista").InnerText;
                    if (NR_Cnpj_Faccionista.Length > 14)
                    {
                        Erro xmlInvalido = new Erro();

                        xmlInvalido.codigo = "1010";
                        xmlInvalido.tipo = "Campo <NR_Cnpj_Faccionista> inválido! Máximo de 14 dígitos!";
                        erroList.Add(xmlInvalido);

                        xml dadosXML = new xml(erroList);
                        return dadosXML;
                    }
                    t = verifica_Lote(idcliente, NR_Nota_Fiscal, DT_Emissao, OP_Tipo_Lote, NR_Cnpj_Faccionista);
                    if (t == null)
                    {
                        vSqL = "INSERT INTO Romaneio_Lote ";
                        vSqL += "(ID_Cliente, NR_Nota_Fiscal, DT_Emissao, OP_Tipo_Lote, NR_Cnpj_Faccionista, ID_Status, DT_Status, COD_Verificacao) ";
                        vSqL += "VALUES(@ID_Cliente, @NR_Nota_Fiscal, @DT_Emissao, @OP_Tipo_Lote, @NR_Cnpj_Faccionista, '1', NOW(), @COD_Verificacao)";
                        Lote lote1 = new Lote();

                        cmd = new MySqlCommand(vSqL, conection);
                        try
                        {
                            cmd.Parameters.AddWithValue("@ID_Cliente", id_cliente);
                            cmd.Parameters.AddWithValue("@NR_Nota_Fiscal", NR_Nota_Fiscal);
                            cmd.Parameters.AddWithValue("@DT_Emissao", DT_Emissao);
                            cmd.Parameters.AddWithValue("@OP_Tipo_Lote", OP_Tipo_Lote);
                            cmd.Parameters.AddWithValue("@NR_Cnpj_Faccionista", NR_Cnpj_Faccionista);
                            cmd.Parameters.AddWithValue("@COD_Verificacao", codigo);

                            cmd.CommandType = CommandType.Text;
                            cmd.ExecuteNonQuery();

                            seleciona = "SELECT LAST_INSERT_ID()";
                            cme = new MySqlCommand(seleciona, conection);

                            ultimoid = Convert.ToInt32(cme.ExecuteScalar());

                            romaneio = itemlote.SelectNodes("Romaneio");
                            List<Romaneio> romaneios = new List<Romaneio>();
                            try
                            {
                                foreach (XmlNode itemromaneio in romaneio)
                                {
                                    string NR_Romaneio = itemromaneio.SelectSingleNode("NR_Romaneio").InnerText;
                                    if (NR_Romaneio == null)
                                    {
                                        Erro xmlInvalido = new Erro();

                                        xmlInvalido.codigo = "1012";
                                        xmlInvalido.tipo = "Campo <NR_Romaneio> vazio!";
                                        erroList.Add(xmlInvalido);

                                        xml dadosXML = new xml(erroList);
                                        return dadosXML;
                                    }
                                    string DC_Artigo = itemromaneio.SelectSingleNode("DC_Artigo").InnerText;
                                    if (DC_Artigo == null)
                                    {
                                        Erro xmlInvalido = new Erro();

                                        xmlInvalido.codigo = "1014";
                                        xmlInvalido.tipo = "Campo <DC_Artigo> vazio!";
                                        erroList.Add(xmlInvalido);

                                        xml dadosXML = new xml(erroList);
                                        return dadosXML;
                                    }
                                    string DC_Cor = itemromaneio.SelectSingleNode("DC_Cor").InnerText;
                                    if (DC_Cor == null)
                                    {
                                        Erro xmlInvalido = new Erro();

                                        xmlInvalido.codigo = "1016";
                                        xmlInvalido.tipo = "Campo <DC_Cor> vazio!";
                                        erroList.Add(xmlInvalido);

                                        xml dadosXML = new xml(erroList);
                                        return dadosXML;
                                    }
                                    string OP_Tipo = itemromaneio.SelectSingleNode("OP_Tipo").InnerText;
                                    if (OP_Tipo == null)
                                    {
                                        Erro xmlInvalido = new Erro();

                                        xmlInvalido.codigo = "1018";
                                        xmlInvalido.tipo = "Campo <OP_Tipo> vazio!";
                                        erroList.Add(xmlInvalido);

                                        xml dadosXML = new xml(erroList);
                                        return dadosXML;
                                    }
                                    string NR_Cod_Produto = itemromaneio.SelectSingleNode("NR_Cod_Produto").InnerText;
                                    if (NR_Cod_Produto == null)
                                    {
                                        Erro xmlInvalido = new Erro();

                                        xmlInvalido.codigo = "1020";
                                        xmlInvalido.tipo = "Campo <NR_Cod_Produto> vazio!";
                                        erroList.Add(xmlInvalido);

                                        xml dadosXML = new xml(erroList);
                                        return dadosXML;
                                    }
                                    string NR_Largura = itemromaneio.SelectSingleNode("NR_Largura").InnerText;
                                    if (NR_Largura == null)
                                    {
                                        Erro xmlInvalido = new Erro();

                                        xmlInvalido.codigo = "1022";
                                        xmlInvalido.tipo = "Campo <NR_Largura> vazio!";
                                        erroList.Add(xmlInvalido);

                                        xml dadosXML = new xml(erroList);
                                        return dadosXML;
                                    }
                                    string NR_Gramatura = itemromaneio.SelectSingleNode("NR_Gramatura").InnerText;
                                    if (NR_Gramatura == null)
                                    {
                                        Erro xmlInvalido = new Erro();

                                        xmlInvalido.codigo = "1024";
                                        xmlInvalido.tipo = "Campo <NR_Gramatura> vazio!";
                                        erroList.Add(xmlInvalido);

                                        xml dadosXML = new xml(erroList);
                                        return dadosXML;
                                    }
                                    string DC_Obs = itemromaneio.SelectSingleNode("DC_Obs").InnerText;
                                    t = verifica_Romaneio(idcliente, NR_Romaneio, DC_Artigo, DC_Cor, OP_Tipo, NR_Cod_Produto, NR_Largura, NR_Gramatura, DC_Obs); //se repetir esses, nao adiciona o Romaneio novo.
                                    if (t == null)
                                    {
                                        vSqL2 = "INSERT INTO Romaneio_Integracao ";
                                        vSqL2 += "(ID_Lote, NR_Romaneio, DC_Artigo, DC_Cor, OP_Tipo, NR_Cod_Produto, NR_Largura, NR_Gramatura, DC_Obs) ";
                                        vSqL2 += "VALUES(@ID_Lote, @NR_Romaneio, @DC_Artigo, @DC_Cor, @OP_Tipo, @NR_Cod_Produto, @NR_Largura, @NR_Gramatura, @DC_Obs)";
                                        Romaneio romaneio1 = new Romaneio();

                                        cmd = new MySqlCommand(vSqL2, conection);
                                        try
                                        {
                                            cmd.Parameters.AddWithValue("@ID_Lote", ultimoid);
                                            cmd.Parameters.AddWithValue("@NR_Romaneio", NR_Romaneio);
                                            cmd.Parameters.AddWithValue("@DC_Artigo", DC_Artigo);
                                            cmd.Parameters.AddWithValue("@DC_Cor", DC_Cor);
                                            cmd.Parameters.AddWithValue("@OP_Tipo", OP_Tipo);
                                            cmd.Parameters.AddWithValue("@NR_Cod_Produto", NR_Cod_Produto);
                                            cmd.Parameters.AddWithValue("@NR_Largura", NR_Largura);
                                            cmd.Parameters.AddWithValue("@NR_Gramatura", NR_Gramatura);
                                            cmd.Parameters.AddWithValue("@DC_Obs", DC_Obs);

                                            cmd.CommandType = CommandType.Text;
                                            cmd.ExecuteNonQuery();

                                            seleciona = "SELECT LAST_INSERT_ID()";
                                            cme = new MySqlCommand(seleciona, conection);

                                            int ultimoid_R = Convert.ToInt32(cme.ExecuteScalar());

                                            peca = itemromaneio.SelectNodes("Peca");
                                            List<Peca> pecas = new List<Peca>();
                                            try
                                            { 
                                                foreach (XmlNode itempeca in peca)
                                                {
                                                    string NR_Peca = itempeca.SelectSingleNode("NR_Peca").InnerText;
                                                    if (NR_Peca == null)
                                                    {
                                                        Erro xmlInvalido = new Erro();

                                                        xmlInvalido.codigo = "1028";
                                                        xmlInvalido.tipo = "Campo <NR_Peca> vazio!";
                                                        erroList.Add(xmlInvalido);

                                                        xml dadosXML = new xml(erroList);
                                                        return dadosXML;
                                                    }
                                                    string NR_Peso = itempeca.SelectSingleNode("NR_Peso").InnerText;
                                                    if (NR_Peso == null)
                                                    {
                                                        Erro xmlInvalido = new Erro();

                                                        xmlInvalido.codigo = "1030";
                                                        xmlInvalido.tipo = "Campo <NR_Peso> vazio!";
                                                        erroList.Add(xmlInvalido);

                                                        xml dadosXML = new xml(erroList);
                                                        return dadosXML;
                                                    }
                                                    string NR_Comprimento = itempeca.SelectSingleNode("NR_Comprimento").InnerText;
                                                    if (NR_Peso == null)
                                                    {
                                                        Erro xmlInvalido = new Erro();

                                                        xmlInvalido.codigo = "1030";
                                                        xmlInvalido.tipo = "Campo <NR_Peso> vazio!";
                                                        erroList.Add(xmlInvalido);

                                                        xml dadosXML = new xml(erroList);
                                                        return dadosXML;
                                                    }


                                                    t = verifica_Peca(ultimoid, NR_Peca, NR_Peso, NR_Comprimento); //se repetir esses, nao adiciona peça nova.

                                                    if (t == null)
                                                    {
                                                        vSqL3 = "INSERT INTO Romaneio_Integracao_Pecas ";
                                                        vSqL3 += "(ID_Romaneio, NR_Peca, NR_Peso, NR_Comprimento) ";
                                                        vSqL3 += "VALUES(@ID_Romaneio, @NR_Peca, @NR_Peso, @NR_Comprimento)";
                                                        Peca peca1 = new Peca();

                                                        cmd = new MySqlCommand(vSqL3, conection);
                                                        try
                                                        {
                                                            cmd.Parameters.AddWithValue("@ID_Romaneio", ultimoid_R);
                                                            cmd.Parameters.AddWithValue("@NR_Peca", NR_Peca);
                                                            cmd.Parameters.AddWithValue("@NR_Peso", NR_Peso);
                                                            cmd.Parameters.AddWithValue("@NR_Comprimento", NR_Comprimento);


                                                            cmd.CommandType = CommandType.Text;

                                                            cmd.ExecuteNonQuery();
                                                            

                                                        }
                                                        catch (Exception erro)
                                                        {
                                                            Trans.Rollback();

                                                            conection.Close();

                                                            List<Erro> erros = new List<Erro>();
                                                            Erro erro1 = new Erro();
                                                            erro1.tipo = erro.ToString();
                                                            erros.Add(erro1);
                                                            xml erroxml = new xml(erros);
                                                            return erroxml;


                                                           
                                                        }
                                                        finally
                                                        {
                                                            conection.Close();
                                                            conection.Dispose();
                                                            Trans.Rollback();
                                                        }
                                                    }
                                                    else
                                                    {
                                                        return t;
                                                    }
                                                }
                                            }
                                            catch(Exception error)
                                            {
                                                Trans.Rollback();
                                                conection.Close();
                                                Erro xmlInvalidoooo = new Erro();

                                                xmlInvalidoooo.codigo = "1027";
                                                xmlInvalidoooo.tipo = "Peça não identificada!";
                                                erroList.Add(xmlInvalidoooo);

                                                xml dadoooosXML = new xml(erroList);
                                                return dadoooosXML;
                                            }
                                        }
                                        catch (Exception erro)
                                        {
                                            Trans.Rollback();
                                            conection.Close();

                                            List<Erro> erros = new List<Erro>();
                                            Erro erro1 = new Erro();
                                            erro1.tipo = erro.ToString();
                                            erros.Add(erro1);
                                            xml erroxml = new xml(erros);
                                            return erroxml;
                                        }

                                        finally
                                        {
                                            conection.Close();
                                            conection.Dispose();
                                            

                                        }
                                    }
                                    else
                                    {
                                        return t;
                                    }
                                }
                            }
                            catch (Exception erro)
                            {
                                Trans.Rollback();
                                conection.Close();
                                Erro xmlInvalidooo = new Erro();

                                xmlInvalidooo.codigo = "1011";
                                xmlInvalidooo.tipo = "Romaneio não identificado!";
                                erroList.Add(xmlInvalidooo);

                                xml dadooosXML = new xml(erroList);
                                return dadooosXML;
                            }

                        }
                        catch (Exception erro)
                        {
                            Trans.Rollback();
                            conection.Close();
                            List<Erro> erros = new List<Erro>();
                            Erro erro1 = new Erro();
                            erro1.tipo = erro.ToString();
                            erros.Add(erro1);
                            xml erroxml = new xml(erros);
                            return erroxml;
                        }
                    }
                    else
                    {
                        return t;
                    }
                }
            }
            catch(Exception error)
            {
                Trans.Rollback();
                conection.Close();
                Erro xmlInvalidoo = new Erro();

                xmlInvalidoo.codigo = "1001";
                xmlInvalidoo.tipo = "Lote não identificado!";
                erroList.Add(xmlInvalidoo);

                xml dadoosXML = new xml(erroList);
                return dadoosXML;
            }
            List<Sucesso> sucessos = new List<Sucesso>();
            Sucesso sucesso = new Sucesso();
            sucesso.tipo = "Sucesso ao Enviar o(s) Lote(s) !";
            sucessos.Add(sucesso);
            xml sucessoxml = new xml(sucessos);
            Trans.Commit();
            conection.Close();
            return sucessoxml;
        }

        [WebMethod(Description = "Artigo e Cor")]
        [SoapDocumentMethod]
        public xml WsArtigoECor(string CNPJ)
        {
            List<Cor> cores = new List<Cor>();

            string vSQL = "SELECT * FROM v_SaldoArtigoCor A";
            vSQL += " INNER JOIN Cadastro_Cliente C ON A.ID_Cliente = C.ID_Cliente ";
            vSQL += " WHERE C.Cnpj = '" + CNPJ + "'";

            using (var connection = new MySqlConnection("Server=seiren_dev.mysql.dbaas.com.br;Port=3306;Database=seiren_dev;Uid=seiren_dev;Pwd=S3iR3n@1973dev;"))
            {
                connection.Open(); using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = connection;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = vSQL;
                    using (var dr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                Cor cor = new Cor();
                                cor.artigo = dr["Artigo"].ToString();
                                cor.divisao = dr["Divisao"].ToString();
                                cor.corl = dr["Cor"].ToString();
                                cor.pecas = Convert.ToDecimal(dr["Pecas"]);
                                cor.peso = Convert.ToDecimal(dr["Peso"]);
                                cores.Add(cor);
                            }
                            dr.Close();
                            dr.Dispose();

                            xml dadosXML = new xml(cores);
                            //Retornar o xml
                            return dadosXML;
                        }
                    }
                }
            }

            return null;
        }

        //[WebMethod(Description = "Busca os lotes")]
        //[SoapDocumentMethod]
        public xml WsBuscaLotes(string CNPJ)
        {
            List<Lote> Lotes = new List<Lote>();

            string vSQL = "";
            vSQL = "SELECT NR_Nota_Fiscal, ID_Lote, Nome, DC_Tipo, NR_Cnpj_Faccionista";
            vSQL += " FROM Romaneio_Lote L ";
            vSQL += " INNER JOIN Romaneio_Tipo T ON L.OP_Tipo_Lote = T.ID_Tipo ";
            vSQL += " INNER JOIN Cadastro_Cliente C ON L.ID_Cliente = C.ID_Cliente ";
            vSQL += " WHERE C.Cnpj = '" + CNPJ + "'";

            using (var conection = new MySqlConnection("Server=seiren_dev.mysql.dbaas.com.br;Port=3306;Database=seiren_dev;Uid=seiren_dev;Pwd=S3iR3n@1973dev;"))
            {
                conection.Open(); using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = conection;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = vSQL; //aqui e executado a interacao com o banco
                    using (var dr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                Lote lote = new Lote();
                                lote.id_lote = Convert.ToInt32(dr["ID_Lote"]);
                                lote.nr_nota_fiscal = dr["NR_Nota_Fiscal"].ToString();
                                lote.nome = dr["Nome"].ToString();
                                lote.dc_tipo = dr["DC_Tipo"].ToString();
                                lote.nr_cnpj_faccionista = dr["NR_Cnpj_Faccionista"].ToString();
                                List<Romaneio> romaneios = new List<Romaneio>();
                                vSQL = "SELECT * FROM Romaneio_Integracao WHERE ID_Lote = " + lote.id_lote;
                                using (var connection = new MySqlConnection("Server=seiren_dev.mysql.dbaas.com.br;Port=3306;Database=seiren_dev;Uid=seiren_dev;Pwd=S3iR3n@1973dev;"))
                                {
                                    connection.Open(); using (var cmdc = new MySqlCommand())
                                    {
                                        cmdc.Connection = connection;
                                        cmdc.CommandType = CommandType.Text;
                                        cmdc.CommandText = vSQL;
                                        using (var dt = cmdc.ExecuteReader(CommandBehavior.CloseConnection))
                                        {
                                            if (dt.HasRows)
                                            {
                                                while (dt.Read())
                                                {
                                                    Romaneio romaneio = new Romaneio();
                                                    romaneio.id_romaneio = Convert.ToInt32(dt["ID_Romaneio"]);
                                                    romaneio.nr_romaneio = dt["NR_Romaneio"].ToString();
                                                    romaneio.dc_artigo = dt["DC_Artigo"].ToString();
                                                    romaneio.dc_cor = dt["DC_Cor"].ToString();
                                                    romaneio.op_tipo = Convert.ToInt32(dt["OP_Tipo"]);
                                                    romaneio.nr_cod_produto = dt["NR_Cod_Produto"].ToString();
                                                    romaneio.nr_largura = Convert.ToDecimal(dt["NR_Largura"]);
                                                    romaneio.nr_gramatura = Convert.ToInt32(dt["NR_Gramatura"]);
                                                    romaneio.dc_obs = dt["DC_Obs"].ToString();
                                                    List<Peca> pecas = new List<Peca>();
                                                    vSQL = "SELECT * FROM Romaneio_Integracao_Pecas WHERE ID_Romaneio = " + romaneio.id_romaneio;
                                                    using (var connnection = new MySqlConnection("Server=seiren_dev.mysql.dbaas.com.br;Port=3306;Database=seiren_dev;Uid=seiren_dev;Pwd=S3iR3n@1973dev;"))
                                                    {
                                                        connnection.Open(); using (var cmdcm = new MySqlCommand())
                                                        {
                                                            cmdcm.Connection = connnection;
                                                            cmdcm.CommandType = CommandType.Text;
                                                            cmdcm.CommandText = vSQL;
                                                            using (var dy = cmdcm.ExecuteReader(CommandBehavior.CloseConnection))
                                                            {
                                                                if (dy.HasRows)
                                                                {
                                                                    while (dy.Read())
                                                                    {
                                                                        Peca peca = new Peca();
                                                                        peca.id_peca = Convert.ToInt32(dy["ID_Peca"]);
                                                                        peca.nr_peca = dy["NR_Peca"].ToString();
                                                                        peca.nr_peso = Convert.ToDecimal(dy["NR_Peso"]);
                                                                        peca.nr_comprimento = Convert.ToDecimal(dy["NR_Comprimento"]);
                                                                        peca.tp_maquina = dy["TP_Maquina"].ToString();
                                                                        pecas.Add(peca);
                                                                    }
                                                                    dy.Close();
                                                                    dy.Dispose();
                                                                }
                                                            }
                                                        }
                                                    }
                                                    romaneio.pecas = pecas;
                                                    romaneios.Add(romaneio);
                                                }
                                                dt.Close();
                                                dt.Dispose();
                                            }
                                        }
                                    }
                                }
                                lote.romaneios = romaneios;
                                Lotes.Add(lote);
                            }
                            dr.Close();
                            dr.Dispose();

                            //Popular a Classe xml
                            xml dadosXML = new xml(Lotes);
                            //Retornar o xml
                            return dadosXML;
                        }
                        return null;
                    }
                }
            }
        }

        ////[WebMethod(Description = "1 - Cartao | 2 - Resumo | 3 - Artigo | 4 - Artigo e Cor | 5 - Nota Fiscal")]
        ////[SoapDocumentMethod]
        public xml WsVerSaldo(string CNPJ, int Opcao)
        {
            string vSQL = "";
            switch (Opcao)
            {
                case 1:
                    List<Cartao> cartoes = new List<Cartao>();

                    vSQL = "SELECT * FROM v_SaldoCartao S ";
                    vSQL += " INNER JOIN Cadastro_Cliente C ON S.ID_Cliente = C.ID_Cliente ";
                    vSQL += " WHERE C.Cnpj = '" + CNPJ + "'";

                    using (var connection = new MySqlConnection("Server=seiren_dev.mysql.dbaas.com.br;Port=3306;Database=seiren_dev;Uid=seiren_dev;Pwd=S3iR3n@1973dev;"))
                    {
                        connection.Open(); using (var cmd = new MySqlCommand())
                        {
                            cmd.Connection = connection;
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = vSQL;
                            using (var dr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                            {
                                if (dr.HasRows)
                                {
                                    while (dr.Read())
                                    {
                                        Cartao cartao = new Cartao();
                                        cartao.cartao = dr["Cartao"].ToString();
                                        cartao.pecas = Convert.ToInt32(dr["Pecas"]);
                                        cartao.peso = Convert.ToDecimal(dr["Peso"]);
                                        cartao.artigo = dr["Artigo"].ToString();
                                        cartao.divisao = dr["Divisao"].ToString();
                                        cartao.cor = dr["Cor"].ToString();
                                        cartao.nota = dr["Nota"].ToString();
                                        cartao.romaneio = dr["Romaneio"].ToString();
                                        cartao.dtentr = dr["DtEntr"].ToString();
                                        cartao.dtcartao = dr["DtCartao"].ToString();
                                        cartao.dtlabor = dr["DtLabor"].ToString();
                                        cartao.dtliber = dr["DtLiber"].ToString();
                                        cartao.pzoentr = dr["PzoEntr"].ToString();
                                        cartao.situacao = dr["Situacao"].ToString();
                                        cartoes.Add(cartao);
                                    }
                                    dr.Close();
                                    dr.Dispose();

                                    xml dadosXML = new xml(cartoes);
                                    //Retornar o xml
                                    return dadosXML;
                                }
                            }
                        }
                    }
                    break;
                case 2:
                    List<Resumo> resumos = new List<Resumo>();

                    vSQL = "SELECT * FROM v_Resumo R";
                    vSQL += " INNER JOIN Cadastro_Cliente C ON R.ID_Cliente = C.ID_Cliente ";
                    vSQL += " WHERE C.Cnpj = '" + CNPJ + "' ORDER BY Id_Ordem_Sit";

                    using (var connection = new MySqlConnection("Server=seiren_dev.mysql.dbaas.com.br;Port=3306;Database=seiren_dev;Uid=seiren_dev;Pwd=S3iR3n@1973dev;"))
                    {
                        connection.Open(); using (var cmd = new MySqlCommand())
                        {
                            cmd.Connection = connection;
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = vSQL;
                            using (var dr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                            {
                                if (dr.HasRows)
                                {
                                    while (dr.Read())
                                    {
                                        Resumo resumo = new Resumo();
                                        resumo.situacao = dr["DC_Situacao"].ToString();
                                        resumo.tpecas = Convert.ToDecimal(dr["TotalPecas"]);
                                        resumo.tpeso = Convert.ToDecimal(dr["TotalPeso"]);
                                        resumos.Add(resumo);
                                    }
                                    dr.Close();
                                    dr.Dispose();

                                    xml dadosXML = new xml(resumos);
                                    //Retornar o xml
                                    return dadosXML;
                                }
                            }
                        }
                    }
                    break;
                case 3:
                    List<Artigo> artigos = new List<Artigo>();

                    vSQL = "SELECT * FROM v_SaldoArtigo A";
                    vSQL += " INNER JOIN Cadastro_Cliente C ON A.ID_Cliente = C.ID_Cliente ";
                    vSQL += " WHERE C.Cnpj = '" + CNPJ + "'";

                    using (var connection = new MySqlConnection("Server=seiren_dev.mysql.dbaas.com.br;Port=3306;Database=seiren_dev;Uid=seiren_dev;Pwd=S3iR3n@1973dev;"))
                    {
                        connection.Open(); using (var cmd = new MySqlCommand())
                        {
                            cmd.Connection = connection;
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = vSQL;
                            using (var dr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                            {
                                if (dr.HasRows)
                                {
                                    while (dr.Read())
                                    {
                                        Artigo artigo = new Artigo();
                                        artigo.artigol = dr["Artigo"].ToString();
                                        artigo.divisao = dr["Divisao"].ToString();
                                        artigo.pecas = Convert.ToDecimal(dr["Pecas"]);
                                        artigo.peso = Convert.ToDecimal(dr["Peso"]);
                                        artigos.Add(artigo);
                                    }
                                    dr.Close();
                                    dr.Dispose();

                                    xml dadosXML = new xml(artigos);
                                    //Retornar o xml
                                    return dadosXML;
                                }
                            }
                        }
                    }
                    break;
                case 4:
                    List<Cor> cores = new List<Cor>();

                    vSQL = "SELECT * FROM v_SaldoArtigoCor A";
                    vSQL += " INNER JOIN Cadastro_Cliente C ON A.ID_Cliente = C.ID_Cliente ";
                    vSQL += " WHERE C.Cnpj = '" + CNPJ + "'";

                    using (var connection = new MySqlConnection("Server=seiren_dev.mysql.dbaas.com.br;Port=3306;Database=seiren_dev;Uid=seiren_dev;Pwd=S3iR3n@1973dev;"))
                    {
                        connection.Open(); using (var cmd = new MySqlCommand())
                        {
                            cmd.Connection = connection;
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = vSQL;
                            using (var dr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                            {
                                if (dr.HasRows)
                                {
                                    while (dr.Read())
                                    {
                                        Cor cor = new Cor();
                                        cor.artigo = dr["Artigo"].ToString();
                                        cor.divisao = dr["Divisao"].ToString();
                                        cor.corl = dr["Cor"].ToString();
                                        cor.pecas = Convert.ToDecimal(dr["Pecas"]);
                                        cor.peso = Convert.ToDecimal(dr["Peso"]);
                                        cores.Add(cor);
                                    }
                                    dr.Close();
                                    dr.Dispose();

                                    xml dadosXML = new xml(cores);
                                    //Retornar o xml
                                    return dadosXML;
                                }
                            }
                        }
                    }
                    break;
                case 5:
                    List<Nota> notas = new List<Nota>();

                    vSQL = "SELECT * FROM v_SaldoNota N";
                    vSQL += " INNER JOIN Cadastro_Cliente C ON N.ID_Cliente = C.ID_Cliente ";
                    vSQL += " WHERE C.Cnpj = '" + CNPJ + "'";

                    using (var connection = new MySqlConnection("Server=seiren_dev.mysql.dbaas.com.br;Port=3306;Database=seiren_dev;Uid=seiren_dev;Pwd=S3iR3n@1973dev;"))
                    {
                        connection.Open(); using (var cmd = new MySqlCommand())
                        {
                            cmd.Connection = connection;
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = vSQL;
                            using (var dr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                            {
                                if (dr.HasRows)
                                {
                                    while (dr.Read())
                                    {
                                        Nota nota = new Nota();
                                        nota.notaf = dr["Nota"].ToString();
                                        nota.dtentr = dr["DtEntr"].ToString();
                                        nota.pecas = Convert.ToDecimal(dr["Pecas"]);
                                        nota.peso = Convert.ToDecimal(dr["Peso"]);
                                        notas.Add(nota);
                                    }
                                    dr.Close();
                                    dr.Dispose();

                                    xml dadosXML = new xml(notas);
                                    //Retornar o xml
                                    return dadosXML;
                                }
                            }
                        }
                    }
                    break;
            }

            return null;
        }


        public xml Campos_Lote(string NR_Nota_Fiscal, string DT_Emissao, string OP_Tipo_Lote, string NR_Cnpj_Faccionista)
        {
            ServiceErrors erroLote = new ServiceErrors();
            List<Lote> erroList = new List<Lote>();
            Lote erro_Lote_Lista = new Lote();
            Lote erro_Lote_Lista1 = new Lote();
            Lote erro_Lote_Lista2 = new Lote();
            Lote erro_Lote_Lista3 = new Lote();
            ContaCaractere contador = new ContaCaractere();
            int cont1 = 0;

            erro_Lote_Lista.erro_Lote = contador.Max9(NR_Nota_Fiscal, "NR_Nota_Fiscal");
            if (erro_Lote_Lista.erro_Lote != null)
                cont1 = 1;

            erro_Lote_Lista1.erro_Lote = contador.Max10(DT_Emissao, "DT_Emissao");
            if (erro_Lote_Lista1.erro_Lote != null)
                cont1 = 2;

            erro_Lote_Lista2.erro_Lote = contador.Max1(OP_Tipo_Lote, "OP_Tipo_Lote");
            if (erro_Lote_Lista2.erro_Lote != null)
                cont1 = 3;

            erro_Lote_Lista3.erro_Lote = contador.Max18(NR_Cnpj_Faccionista, "NR_Cnpj_Faccionista");
            if (erro_Lote_Lista3.erro_Lote != null)
                cont1 = 4;
            if (cont1 == 1)
            {
                erroList.Add(erro_Lote_Lista);
                xml dadosXML = new xml(erroList);
                return dadosXML;
            }
            if (cont1 == 2)
            {
                erroList.Add(erro_Lote_Lista1);
                xml dadosXML = new xml(erroList);
                return dadosXML;
            }
            if (cont1 == 3)
            {
                erroList.Add(erro_Lote_Lista2);
                xml dadosXML = new xml(erroList);
                return dadosXML;
            }
            if (cont1 == 4)
            {
                erroList.Add(erro_Lote_Lista3);
                xml dadosXML = new xml(erroList);
                return dadosXML;
            }

            return null;

        }
        public xml verifica_Lote(string id, string NR_Nota_Fiscal, string DT_Emissao, string OP_Tipo_Lote, string NR_Cnpj_Faccionista)
        {
            MySqlConnection con = null;
            ServiceErrors erroLote = new ServiceErrors();
            List<Lote> erroList = new List<Lote>();

            if (Convert.ToInt32(OP_Tipo_Lote) == 1)
            {
                if (NR_Cnpj_Faccionista != "0")
                {


                    Lote erro_Lote_Lista = new Lote();


                    erro_Lote_Lista.erro_Lote = erroLote.ErrorMessageTipo;
                    erro_Lote_Lista.CodigoErro = "1009";

                    erroList.Add(erro_Lote_Lista);
                    xml dadosXML = new xml(erroList);
                    return dadosXML;
                }
            }
            else if (Convert.ToInt32(OP_Tipo_Lote) == 2 || Convert.ToInt32(OP_Tipo_Lote) == 3)
            {
                if (NR_Cnpj_Faccionista == "0")
                {
                    Lote erro_Lote_Lista = new Lote();


                    erro_Lote_Lista.erro_Lote = "Campo <NR_Cnpj_Faccionista> vazio, enquanto <OP_Tipo_Lote> diferente de '1'";
                    erro_Lote_Lista.CodigoErro = "1008";

                    erroList.Add(erro_Lote_Lista);
                    xml dadosXML = new xml(erroList);
                    return dadosXML;
                }
            }
            else
            {
                Erro xmlInvalido = new Erro();

                Lote erro_Lote_Lista = new Lote();


                erro_Lote_Lista.erro_Lote = "Campo <OP_Tipo_Lote> inválido! [1 - 3]";
                erro_Lote_Lista.CodigoErro = "1007";

                erroList.Add(erro_Lote_Lista);
                xml dadosXML = new xml(erroList);
                return dadosXML;
            }

            xml t = Campos_Lote(NR_Nota_Fiscal, DT_Emissao, OP_Tipo_Lote, NR_Cnpj_Faccionista);

            if (t == null)
            {

                try
                {

                    string leitura = "SELECT COUNT(*) FROM Romaneio_Lote L INNER JOIN Cadastro_Cliente C ON L.ID_Cliente =" + id + "  WHERE NR_Nota_Fiscal = " + (NR_Nota_Fiscal) + " AND NR_Cnpj_Faccionista = '" + (NR_Cnpj_Faccionista) + "'";
                    using (var connection = new MySqlConnection("Server=seiren_dev.mysql.dbaas.com.br;Port=3306;Database=seiren_dev;Uid=seiren_dev;Pwd=S3iR3n@1973dev;"))
                    {
                        connection.Open(); using (var cmd = new MySqlCommand())
                        {
                            cmd.Connection = connection;
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = leitura; //aqui e executado a interacao com o banco
                            string aviso = "";
                            int cont = 0;

                            cont = Convert.ToInt32(cmd.ExecuteScalar());
                            connection.Close();

                            if (cont < 1)
                            {
                                return null;
                            }
                            else
                            {
                                Lote erro_lote_Lista = new Lote();
                                erro_lote_Lista.erro_Lote = "Campo <NR_Nota_Fiscal> repetido ou inválido! Máximo de 9 dígitos!";
                                erro_lote_Lista.CodigoErro = "1003";
                                erro_lote_Lista.dc_tipo = OP_Tipo_Lote;
                                erro_lote_Lista.nr_cnpj_faccionista = NR_Cnpj_Faccionista;
                                erro_lote_Lista.nr_nota_fiscal = NR_Nota_Fiscal;
                                erroList.Add(erro_lote_Lista);
                                xml dadosXML = new xml(erroList);
                                return dadosXML;
                            }

                        }
                    }
                }
                catch (MySqlException erro)
                {
                    throw erro;
                }
            }
            else
            {
                return t;
            }
        }
        public xml Campos_Romaneio(string NR_Romaneio, string DC_Artigo, string DC_Cor, string OP_Tipo, string NR_Cod_Produto, string NR_Largura, string NR_Gramatura, string DC_Obs)
        {
            MySqlConnection con = null;

            ServiceErrors erroRomaneio = new ServiceErrors();
            List<Romaneio> erroList = new List<Romaneio>();
            Romaneio erro_Rom_Lista = new Romaneio();
            Romaneio erro_Rom_Lista1 = new Romaneio();
            Romaneio erro_Rom_Lista2 = new Romaneio();
            Romaneio erro_Rom_Lista3 = new Romaneio();
            Romaneio erro_Rom_Lista4 = new Romaneio();
            Romaneio erro_Rom_Lista5 = new Romaneio();
            Romaneio erro_Rom_Lista6 = new Romaneio();
            Romaneio erro_Rom_Lista7 = new Romaneio();
            ContaCaractere contador = new ContaCaractere();
            int cont1 = 0;

            erro_Rom_Lista.erro_Romaneio = contador.Max7(NR_Romaneio, "NR_Romaneio");
            if (erro_Rom_Lista.erro_Romaneio != null)
                cont1 = 1;

            erro_Rom_Lista1.erro_Romaneio = contador.Max16(DC_Artigo, "DC_Artigo");
            if (erro_Rom_Lista1.erro_Romaneio != null)
                cont1 = 2;

            erro_Rom_Lista2.erro_Romaneio = contador.Max45(DC_Cor, "DC_Cor");
            if (erro_Rom_Lista2.erro_Romaneio != null)
                cont1 = 3;

            erro_Rom_Lista3.erro_Romaneio = contador.Max1(OP_Tipo, "OP_Tipo");
            if (erro_Rom_Lista3.erro_Romaneio != null)
                cont1 = 4;

            erro_Rom_Lista4.erro_Romaneio = contador.Max25(NR_Cod_Produto, "NR_Cod_Produto");
            if (erro_Rom_Lista4.erro_Romaneio != null)
                cont1 = 5;

            erro_Rom_Lista5.erro_Romaneio = contador.Max4(NR_Largura, "NR_Largura");
            if (erro_Rom_Lista5.erro_Romaneio != null)
                cont1 = 6;

            erro_Rom_Lista6.erro_Romaneio = contador.Max3(NR_Gramatura, "NR_Gramatura");
            if (erro_Rom_Lista6.erro_Romaneio != null)
                cont1 = 7;

            erro_Rom_Lista7.erro_Romaneio = contador.Max50(DC_Obs, "Dc_Obs");
            if (erro_Rom_Lista7.erro_Romaneio != null)
                cont1 = 8;

            //erro_Rom_Lista.erro_Romaneio = contador.MaxVirgula;


            if (cont1 == 1)
            {
                erroList.Add(erro_Rom_Lista);
                xml dadosXML = new xml(erroList);
                return dadosXML;
            }
            if (cont1 == 2)
            {
                erroList.Add(erro_Rom_Lista1);
                xml dadosXML = new xml(erroList);
                return dadosXML;
            }
            if (cont1 == 3)
            {
                erroList.Add(erro_Rom_Lista2);
                xml dadosXML = new xml(erroList);
                return dadosXML;
            }
            if (cont1 == 4)
            {
                erroList.Add(erro_Rom_Lista3);
                xml dadosXML = new xml(erroList);
                return dadosXML;
            }
            if (cont1 == 5)
            {
                erroList.Add(erro_Rom_Lista4);
                xml dadosXML = new xml(erroList);
                return dadosXML;
            }
            if (cont1 == 6)
            {
                erroList.Add(erro_Rom_Lista5);
                xml dadosXML = new xml(erroList);
                return dadosXML;
            }
            if (cont1 == 7)
            {
                erroList.Add(erro_Rom_Lista6);
                xml dadosXML = new xml(erroList);
                return dadosXML;
            }
            if (cont1 == 8)
            {
                erro_Rom_Lista7.CodigoErro = "1026";
                erroList.Add(erro_Rom_Lista7);
                xml dadosXML = new xml(erroList);
                return dadosXML;
            }

            return null;

        }
        public xml verifica_Romaneio(string id, string NR_Romaneio, string DC_Artigo, string DC_Cor, string OP_Tipo, string NR_Cod_Produto, string NR_Largura, string NR_Gramatura, string DC_Obs)
        {
            MySqlConnection con = null;

            ServiceErrors erroRomaneio = new ServiceErrors();
            List<Romaneio> erroList = new List<Romaneio>();

            ContaCaractere contador = new ContaCaractere();

            if (Convert.ToInt32(NR_Largura) < 0.4 || Convert.ToInt32(NR_Largura) > 3)
            {
                Romaneio erro_Rom_Lista = new Romaneio();


                erro_Rom_Lista.erro_Romaneio = erroRomaneio.ErrorMessageLimiteLargura;
                erro_Rom_Lista.CodigoErro = "1023";

                erroList.Add(erro_Rom_Lista);
                xml dadosXML = new xml(erroList);
                return dadosXML;
            }
            if (Convert.ToInt32(NR_Gramatura) < 0 || Convert.ToInt32(NR_Gramatura) > 600)
            {
                Romaneio erro_Rom_Lista = new Romaneio();


                erro_Rom_Lista.erro_Romaneio = erroRomaneio.ErrorMessageLimiteGramatura;
                erro_Rom_Lista.CodigoErro = "1025";

                erroList.Add(erro_Rom_Lista);
                xml dadosXML = new xml(erroList);
                return dadosXML;
            }
            if (Convert.ToInt32(OP_Tipo) < 0 || Convert.ToInt32(OP_Tipo) > 3)
            {
                Romaneio erro_Rom_Lista = new Romaneio();


                erro_Rom_Lista.erro_Romaneio = erroRomaneio.ErrorMessageLimiteTipo;
                erro_Rom_Lista.CodigoErro = "1019";

                erroList.Add(erro_Rom_Lista);
                xml dadosXML = new xml(erroList);
                return dadosXML;
            }

            xml x = Campos_Romaneio(NR_Romaneio, DC_Artigo, DC_Cor, OP_Tipo, NR_Cod_Produto, NR_Largura, NR_Gramatura, DC_Obs);

            if (x == null)
            {
                try
                {
                    string leitura = "SELECT COUNT(*) FROM Romaneio_Integracao I INNER JOIN Romaneio_Lote L ON I.ID_Lote = L.ID_LOTE WHERE I.DT_DELETE IS NULL AND NR_Romaneio = " + (NR_Romaneio) + " AND DC_Artigo = '" + (DC_Artigo) + "' AND DC_Cor = '" + (DC_Cor) + "' AND OP_Tipo = '" + (OP_Tipo) + "' AND NR_Cod_Produto = '" + (NR_Cod_Produto) + "'";
                    using (var connection = new MySqlConnection("Server=seiren_dev.mysql.dbaas.com.br;Port=3306;Database=seiren_dev;Uid=seiren_dev;Pwd=S3iR3n@1973dev;"))
                    {
                        List<Cor> cores = new List<Cor>();
                        connection.Open(); using (var cmd = new MySqlCommand())
                        {
                            cmd.Connection = connection;
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = leitura; //aqui e executado a interacao com o banco
                            string aviso = "";
                            int cont = 0;


                            cont = Convert.ToInt32(cmd.ExecuteScalar());
                            connection.Close();

                            if (cont < 1)
                            {
                                leitura = "SELECT * FROM v_SaldoArtigoCor WHERE ID_Cliente = '" + id + "'";
                                using (var connecCtion = new MySqlConnection("Server=seiren_dev.mysql.dbaas.com.br;Port=3306;Database=seiren_dev;Uid=seiren_dev;Pwd=S3iR3n@1973dev;"))
                                {
                                    connecCtion.Open(); using (var cCmd = new MySqlCommand())
                                    {
                                        cCmd.Connection = connecCtion;
                                        cCmd.CommandType = CommandType.Text;
                                        cCmd.CommandText = leitura; //aqui e executado a interacao com o banco
                                        using (var dr = cCmd.ExecuteReader(CommandBehavior.CloseConnection))
                                        {
                                            if (dr.HasRows)
                                            {
                                                while (dr.Read())
                                                {
                                                    Cor cor = new Cor();
                                                    cor.artigo = dr["Artigo"].ToString();
                                                    cor.corl = dr["Cor"].ToString();
                                                    cores.Add(cor);
                                                }
                                                dr.Close();
                                                dr.Dispose();
                                                int contacor = 0;
                                                int contaartigo = 0;
                                                foreach (var i in cores) //to tentando corrigir isso agr 17/10/18 10:30am
                                                {
                                                    if (DC_Artigo == i.artigo)
                                                        contaartigo++;
                                                    if (DC_Cor == i.corl)
                                                        contacor++;
                                                }
                                                if (contacor == 0)
                                                {
                                                    Romaneio erro_Rom_Lista = new Romaneio();
                                                    erro_Rom_Lista.erro_Romaneio = "Campo <DC_Cor> inválido, favor consultar sua tabela de cor.";
                                                    erro_Rom_Lista.CodigoErro = "1013";

                                                    erroList.Add(erro_Rom_Lista);
                                                    xml dadossXML = new xml(erroList);
                                                    return dadossXML;
                                                }
                                                if (contaartigo == 0)
                                                {
                                                    Romaneio erro_Rom_Lista = new Romaneio();
                                                    erro_Rom_Lista.erro_Romaneio = "Campo <DC_Artigo> inválido, favor consultar sua tabela de artigos.";
                                                    erro_Rom_Lista.CodigoErro = "1015";

                                                    erroList.Add(erro_Rom_Lista);
                                                    xml dadossXML = new xml(erroList);
                                                    return dadossXML;
                                                }
                                            }
                                        }
                                    }
                                }
                                return null;
                            }
                            else
                            {
                                Romaneio erro_Romaneio_Lista = new Romaneio();
                                erro_Romaneio_Lista.erro_Romaneio = "Campo <NR_Romaneio> repetido ou inválido! Máximo de 7 dígitos!";
                                erro_Romaneio_Lista.CodigoErro = "1013";
                                erro_Romaneio_Lista.dc_artigo = DC_Artigo;
                                erro_Romaneio_Lista.dc_cor = DC_Cor;
                                erro_Romaneio_Lista.dc_obs = DC_Obs;
                                erro_Romaneio_Lista.nr_cod_produto = NR_Cod_Produto;
                                erro_Romaneio_Lista.nr_romaneio = NR_Romaneio;
                                erroList.Add(erro_Romaneio_Lista);
                                xml dadosXML = new xml(erroList);
                                return dadosXML;
                            }
                        }
                    }
                }
                catch (MySqlException erro)
                {
                    throw erro;
                }
            }
            else
            {
                return x;

            }
        }
        public xml Campos_Peca(string NR_Peca, string NR_Peso, string NR_Comprimento)
        {
            MySqlConnection con = null;

            ServiceErrors erroRomaneio = new ServiceErrors();
            List<Peca> erroList = new List<Peca>();
            Peca erro_Peca_Lista = new Peca();
            Peca erro_Peca_Lista1 = new Peca();
            Peca erro_Peca_Lista2 = new Peca();
            Peca erro_Peca_Lista3 = new Peca();
            ContaCaractere contador = new ContaCaractere();
            int cont1 = 0;

            erro_Peca_Lista.erro_Peca = contador.Max10(NR_Peca, "NR_Peca");
            if (erro_Peca_Lista.erro_Peca != null)
                cont1 = 1;

            erro_Peca_Lista1.erro_Peca = contador.Max7(NR_Peso, "NR_Peso");
            if (erro_Peca_Lista1.erro_Peca != null)
                cont1 = 2;

            erro_Peca_Lista2.erro_Peca = contador.Max7(NR_Comprimento, "NR_Comprimento");
            if (erro_Peca_Lista2.erro_Peca != null)
                cont1 = 3;



            if (cont1 == 1)
            {
                erroList.Add(erro_Peca_Lista);
                xml dadosXML = new xml(erroList);
                return dadosXML;
            }
            if (cont1 == 2)
            {
                erroList.Add(erro_Peca_Lista1);
                xml dadosXML = new xml(erroList);
                return dadosXML;
            }
            if (cont1 == 3)
            {
                erroList.Add(erro_Peca_Lista2);
                xml dadosXML = new xml(erroList);
                return dadosXML;
            }
            if (cont1 == 4)
            {
                erroList.Add(erro_Peca_Lista3);
                xml dadosXML = new xml(erroList);
                return dadosXML;
            }

            return null;

        }
        public xml verifica_Peca(int ID_Lote, string NR_Peca, string NR_Peso, string NR_Comprimento)
        {
            MySqlConnection con = null;

            ServiceErrors erroPeca = new ServiceErrors();
            List<Peca> erroList = new List<Peca>();

            ContaCaractere contador = new ContaCaractere();
            int peso = Convert.ToInt32(NR_Peso);
            int comprimento = Convert.ToInt32(NR_Comprimento);

            if (Convert.ToInt32(NR_Peso) < 0.3 || Convert.ToInt32(NR_Peso) > 160)
            {
                Peca erro_Peca_Lista = new Peca();


                erro_Peca_Lista.erro_Peca = erroPeca.ErrorMessageLimitePeso;
                erro_Peca_Lista.CodigoErro = "1031";

                erroList.Add(erro_Peca_Lista);
                xml dadosXML = new xml(erroList);
                return dadosXML;
            }
            if (Convert.ToInt32(NR_Comprimento) < 3 || Convert.ToInt32(NR_Comprimento) > 1200)
            {
                Peca erro_Peca_Lista = new Peca();


                erro_Peca_Lista.erro_Peca = erroPeca.ErrorMessageLimiteGramatura;
                erro_Peca_Lista.CodigoErro = "1033";

                erroList.Add(erro_Peca_Lista);
                xml dadosXML = new xml(erroList);
                return dadosXML;
            }

            xml x = Campos_Peca(NR_Peca, NR_Peso, NR_Comprimento);

            if (x == null)
            {
                try
                {
                    string leitura = "SELECT COUNT(*) FROM Romaneio_Integracao_Pecas P INNER JOIN Romaneio_Integracao I ON P.ID_Romaneio = I.ID_Romaneio WHERE P.NR_Peca = '" + (NR_Peca) + "' AND I.ID_Lote = '" + ID_Lote + "'";
                    using (var connection = new MySqlConnection("Server=seiren_dev.mysql.dbaas.com.br;Port=3306;Database=seiren_dev;Uid=seiren_dev;Pwd=S3iR3n@1973dev;"))
                    {
                        connection.Open(); using (var cmd = new MySqlCommand())
                        {
                            cmd.Connection = connection;
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = leitura; //aqui e executado a interacao com o banco
                            string aviso = "";
                            int cont = 0;

                            cont = Convert.ToInt32(cmd.ExecuteScalar());
                            connection.Close();

                            if (cont < 1)
                            {
                                return null;
                            }
                            else
                            {
                                Peca erro_Peca_Lista = new Peca();
                                erro_Peca_Lista.erro_Peca = "Campo <NR_Peca> repetido ou inválido! Máximo de 10 dígitos!";
                                erro_Peca_Lista.nr_comprimento = comprimento;
                                erro_Peca_Lista.nr_peca = NR_Peca;
                                erro_Peca_Lista.id_lote = ID_Lote;
                                erroList.Add(erro_Peca_Lista);
                                xml dadosXML = new xml(erroList);
                                return dadosXML;
                            }
                        }
                    }
                }
                catch (MySqlException erro)
                {
                    throw erro;
                }
            }
            else
            {
                return x;
            }
        }


        [XmlRoot("xml")]
        public class xml
        {
            public xml() { }
            public xml(List<Lote> lotes)
            {
                this.lotes = lotes;
            }
            public List<Lote> lotes { get; set; }

            public xml(List<Romaneio> romaneios)
            {
                this.romaneios = romaneios;
            }
            public List<Romaneio> romaneios { get; set; }

            public xml(List<Peca> Pecas)
            {
                this.Pecas = Pecas;
            }
            public List<Peca> Pecas { get; set; }

            public xml(List<Cartao> cartoes)
            {
                this.cartoes = cartoes;
            }
            public List<Cartao> cartoes { get; set; }

            public xml(List<Resumo> resumos)
            {
                this.resumos = resumos;
            }
            public List<Resumo> resumos { get; set; }

            public xml(List<Artigo> artigos)
            {
                this.artigos = artigos;
            }
            public List<Artigo> artigos { get; set; }

            public xml(List<Cor> cores)
            {
                this.cores = cores;
            }
            public List<Cor> cores { get; set; }

            public xml(List<Nota> notas)
            {
                this.notas = notas;
            }
            public List<Nota> notas { get; set; }

            public xml(List<Erro> erros)
            {
                this.erros = erros;
            }
            public List<Erro> erros { get; set; }

            public xml(List<Sucesso> sucessos)
            {
                this.sucessos = sucessos;
            }
            public List<Sucesso> sucessos { get; set; }


        }

        public class Sucesso
        {
            [XmlElement("Sucesso")]
            public string tipo { get; set; }
        }

        public class Erro
        {
            [XmlElement("CódigoErro")]
            public string codigo { get; set; }


            [XmlElement("TipoErro")]
            public string tipo { get; set; }
        }

        public class Lote
        {
            [XmlAttribute("ativo")]
            public bool ativo = true;

            [XmlElement("ID_Lote")]
            public int id_lote { get; set; }

            [XmlElement("Nome")]
            public string nome { get; set; }

            [XmlElement("NR_Nota_Fiscal")]
            public string nr_nota_fiscal { get; set; }

            [XmlElement("DC_Tipo")]
            public string dc_tipo { get; set; }

            [XmlElement("NR_Cnpj_Faccionista")]
            public string nr_cnpj_faccionista { get; set; }

            [XmlElement("Romaneios")]
            public List<Romaneio> romaneios { get; set; }

            [XmlElement("Ocorreu_Um_Erro")]
            public string erro_Lote { get; set; }

            [XmlElement("CódigoErro")]
            public string CodigoErro { get; set; }

            //[XmlElement("DT_Emissao")]  
            //public DateTime DT_Emissao { get; set; } nao aceita por ser um time date time
        }

        public class Romaneio
        {
            [XmlAttribute("ativo")]
            public bool ativo = true;

            [XmlElement("ID_Romaneio")]
            public int id_romaneio { get; set; }

            [XmlElement("NR_Romaneio")]
            public string nr_romaneio { get; set; }

            [XmlElement("DC_Artigo")]
            public string dc_artigo { get; set; }

            [XmlElement("DC_Cor")]
            public string dc_cor { get; set; }

            [XmlElement("OP_Tipo")]
            public int op_tipo { get; set; }

            [XmlElement("NR_Cod_Produto")]
            public string nr_cod_produto { get; set; }

            [XmlElement("NR_Largura")]
            public decimal nr_largura { get; set; }

            [XmlElement("NR_Gramatura")]
            public int nr_gramatura { get; set; }

            [XmlElement("DC_Obs")]
            public string dc_obs { get; set; }

            [XmlElement("Pecas")]
            public List<Peca> pecas { get; set; }

            [XmlElement("Ocorreu_Um_Erro")]
            public string erro_Romaneio { get; set; }

            [XmlElement("CódigoErro")]
            public string CodigoErro { get; set; }
        }

        public class Peca
        {
            [XmlAttribute("ativo")]
            public bool ativo = true;

            [XmlElement("ID_Peca")]
            public int id_peca { get; set; }

            [XmlElement("ID_Lote")]
            public int id_lote { get; set; }

            [XmlElement("NR_Peca")]
            public string nr_peca { get; set; }

            [XmlElement("NR_Peso")]

            public decimal nr_peso { get; set; }

            [XmlElement("NR_Comprimento")]
            public decimal nr_comprimento { get; set; }

            [XmlElement("TP_Maquina")]

            public string tp_maquina { get; set; }

            [XmlElement("Ocorreu_Um_Erro")]
            public string erro_Peca { get; set; }

            [XmlElement("CódigoErro")]
            public string CodigoErro { get; set; }
        }

        public class Cartao
        {
            [XmlAttribute("ativo")]
            public bool ativo = true;

            [XmlElement("Cartao")]
            public string cartao { get; set; }

            [XmlElement("Pecas")]
            public int pecas { get; set; }

            [XmlElement("Peso")]
            public decimal peso { get; set; }

            [XmlElement("Artigo")]
            public string artigo { get; set; }

            [XmlElement("Divisao")]
            public string divisao { get; set; }

            [XmlElement("Cor")]
            public string cor { get; set; }

            [XmlElement("Nota")]
            public string nota { get; set; }

            [XmlElement("Romaneio")]
            public string romaneio { get; set; }

            [XmlElement("DataEntrada")]
            public string dtentr { get; set; }

            [XmlElement("DataCartao")]
            public string dtcartao { get; set; }

            [XmlElement("DataLaboratorio")]
            public string dtlabor { get; set; }

            [XmlElement("DataLiberacao")]
            public string dtliber { get; set; }

            [XmlElement("PrazoEntrada")]
            public string pzoentr { get; set; }

            [XmlElement("Situacao")]
            public string situacao { get; set; }
        }

        public class Resumo
        {
            [XmlAttribute("ativo")]
            public bool ativo = true;

            [XmlElement("DC_Situacao")]
            public string situacao { get; set; }

            [XmlElement("TotalPecas")]
            public decimal tpecas { get; set; }

            [XmlElement("TotalPeso")]
            public decimal tpeso { get; set; }
        }

        public class Artigo
        {
            [XmlAttribute("ativo")]
            public bool ativo = true;

            [XmlElement("Artigo")]
            public string artigol { get; set; }

            [XmlElement("Divisao")]
            public string divisao { get; set; }

            [XmlElement("Pecas")]
            public decimal pecas { get; set; }

            [XmlElement("Peso")]
            public decimal peso { get; set; }
        }

        public class Cor
        {
            [XmlAttribute("ativo")]
            public bool ativo = true;

            [XmlElement("Artigo")]
            public string artigo { get; set; }

            [XmlElement("Divisao")]
            public string divisao { get; set; }

            [XmlElement("Cor")]
            public string corl { get; set; }

            [XmlElement("Pecas")]
            public decimal pecas { get; set; }

            [XmlElement("Peso")]
            public decimal peso { get; set; }
        }

        public class Nota
        {
            [XmlAttribute("ativo")]
            public bool ativo = true;

            [XmlElement("NotaFiscal")]
            public string notaf { get; set; }

            [XmlElement("DataEntrada")]
            public string dtentr { get; set; }

            [XmlElement("Pecas")]
            public decimal pecas { get; set; }

            [XmlElement("Peso")]
            public decimal peso { get; set; }
        }
    }
}
