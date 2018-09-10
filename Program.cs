using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace WSconsultaTRM
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                DateTime fechaSap = DateTime.Now.AddDays(1);
                WebServiceTRMReference.TCRMServicesInterfaceClient ws = new WebServiceTRMReference.TCRMServicesInterfaceClient();
                WebServiceTRMReference.tcrmResponse respuestaTRM = new WebServiceTRMReference.tcrmResponse();
                respuestaTRM =  ws.queryTCRM(fechaSap);                

                SAPbobsCOM.Company oCompany = new SAPbobsCOM.Company();
                oCompany.Server = ConfigurationManager.AppSettings.Get("Server");
                oCompany.LicenseServer = ConfigurationManager.AppSettings.Get("LicenseServer");
                oCompany.CompanyDB = ConfigurationManager.AppSettings.Get("CompanyDB");
                oCompany.UserName = ConfigurationManager.AppSettings.Get("UserName");
                oCompany.Password = ConfigurationManager.AppSettings.Get("Password");

                switch (ConfigurationManager.AppSettings.Get("DbServerType"))
                {
                    case "dst_HANADB":
                        oCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_HANADB;
                        break;
                    case "dst_MSSQL2005":
                        oCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2005;
                        break;
                    case "dst_MSSQL2008":
                        oCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2008;
                        break;
                    case "dst_MSSQL2012":
                        oCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2012;
                        break;
                    case "dst_MSSQL2014":
                        oCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2014;
                        break;
                    case "dst_MSSQL2016":
                        oCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2016;
                        break;                    
                }

                

                int resultado = oCompany.Connect();
                if (resultado == 0)
                {
                    SAPbobsCOM.SBObob bo = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoBridge);
                    bo.SetCurrencyRate("USD", fechaSap, Math.Round(respuestaTRM.value, 2), true);
                    
                    oCompany.Disconnect();

                    Console.WriteLine("TRM Actualizada!");
                    string asunto = "TRM SAP "+ ConfigurationManager.AppSettings.Get("CompanyDB") + " Actualizada " + fechaSap.ToString().Substring(0,10);
                    string mensaje = "Se ha realizado correctamente la actualizacion de la tasa de cambio del dolar para el dia " + fechaSap.ToString().Substring(0, 10) + " - TRM: $" + respuestaTRM.value;
                    //envia el correo solo en caso de existir error de actualizacion
                    if (ConfigurationManager.AppSettings.Get("CorreoErrorActualiza") == "NO")
                        EnviarCorreo(asunto, mensaje);
                    Environment.Exit(1);
                }
                else
                {
                    throw new Exception(oCompany.GetLastErrorCode() + ": " + oCompany.GetLastErrorDescription());
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine("Error: " + ex.Message );                
                string asunto = "TRM SAP " + ConfigurationManager.AppSettings.Get("CompanyDB") + " *ERROR* " + DateTime.Now.AddDays(1); 
                string mensaje = "Error en la actualizacion: " + DateTime.Now.AddDays(1)+ " **"+ex.Message;
                EnviarCorreo(asunto, mensaje);
            }
        }


        private static void EnviarCorreo(string asunto, string mensaje )
        {
                         
            try
            {
                string[] TO = ConfigurationManager.AppSettings.Get("correoDestino").Split(';');
                //si no existen correos registrados en el archivo de configuracion no realiza la notificacionn
                if (!String.IsNullOrEmpty(TO[0]))
                {
                    string HOST = ConfigurationManager.AppSettings.Get("smtp");
                    string PORT = ConfigurationManager.AppSettings.Get("puerto");
                    string SMTP_USERNAME = ConfigurationManager.AppSettings.Get("correoSmtp");
                    string SMTP_PASSWORD = ConfigurationManager.AppSettings.Get("claveSmtp");
                    string FROM = ConfigurationManager.AppSettings.Get("correoSmtp");
                    string SUBJECT = asunto;
                    string BODY = "<h1>Consumo Servicio Web SuperFinanciera</h1><p>"+mensaje+"</p>";
                
                    MailMessage message = new MailMessage();
                    message.IsBodyHtml = true;
                    message.From = new MailAddress(FROM);
                    foreach (var item in TO)
                    {
                        message.To.Add(new MailAddress(item));
                    }
                    message.Subject = SUBJECT;
                    message.Body = BODY;
                            
                    SmtpClient client = new SmtpClient(HOST, Convert.ToInt32(PORT));            
                    client.Credentials = new NetworkCredential(SMTP_USERNAME, SMTP_PASSWORD);            
                    client.EnableSsl = true;   
                
                    client.Send(message);
                    Console.WriteLine("Correo enviado!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("El correo no fue enviado");
                Console.WriteLine("Error: " + ex.Message);
            }  
        
        }

    }
}
