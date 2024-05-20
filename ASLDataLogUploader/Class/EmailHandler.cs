using System;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Mail;

namespace ASLDataLogUploader.Class
{
    public class EmailHandler
    {
        public DataSet GetMailRecipients(int autoEmailCode)
        {
            DataSet dsEmail = new DataSet();
            string strSQL = "usp_SPT_AutoEmail_GetRecipients";
            SQLHandler sqlHandler = new SQLHandler();
            sqlHandler.CreateParameter(1);
            sqlHandler.SetParameterValues(0, "@AutoEmailCode", SqlDbType.BigInt, autoEmailCode);

            if (sqlHandler.FillDataSet(strSQL, out dsEmail, CommandType.StoredProcedure))
            {
            }
            sqlHandler = null;
            return dsEmail;
        }

        public void SendEmail(string strSubject, string strMessage, string strFile, DataSet dsEmail)
        {
            try
            {
                MailMessage mailMsg = new MailMessage();
                mailMsg.IsBodyHtml = true;
                mailMsg.Subject = strSubject.Trim();
                mailMsg.Body = strMessage.Trim() + "\n";
                mailMsg.Priority = MailPriority.High;
                mailMsg.IsBodyHtml = true;

                if (!string.IsNullOrEmpty(strFile) && File.Exists(strFile))
                {
                    Attachment msgAttach = new Attachment(strFile);
                    mailMsg.Attachments.Add(msgAttach);
                }

                foreach (DataRow row in dsEmail.Tables[0].Rows)
                {
                    if (Convert.ToBoolean(row["EMailTo"]))
                    {
                        mailMsg.To.Add(new MailAddress(row["Email_Address"].ToString().Trim()));
                    }
                    else if (Convert.ToBoolean(row["EMailCC"]))
                    {
                        mailMsg.CC.Add(new MailAddress(row["Email_Address"].ToString().Trim()));
                    }
                    else if (Convert.ToBoolean(row["EMailBCC"]))
                    {
                        mailMsg.Bcc.Add(new MailAddress(row["Email_Address"].ToString().Trim()));
                    }
                    else if (Convert.ToBoolean(row["EMailFrom"]))
                    {
                        mailMsg.From = new MailAddress(row["Email_Address"].ToString().Trim());
                    }
                }

                SQLHandler sqlHandler = new SQLHandler();
                DataSet ds = new DataSet();
                string strSQL = "usp_Get_ATEC_EmailServer_V2";
                string username, password;
                SmtpClient smtpMail = new SmtpClient();

                sqlHandler.CreateParameter(1);
                sqlHandler.SetParameterValues(0, "@ID", SqlDbType.Int, 1);

                if (sqlHandler.OpenConnection())
                {
                    if (sqlHandler.FillDataSet(strSQL, out ds, CommandType.Text))
                    {
                        smtpMail.Host = ds.Tables[0].Rows[0]["Host"].ToString();
                        smtpMail.Port = Convert.ToInt32(ds.Tables[0].Rows[0]["Port"].ToString());
                        username = ds.Tables[0].Rows[0]["Username"].ToString();
                        password = ds.Tables[0].Rows[0]["Password"].ToString();
                        smtpMail.UseDefaultCredentials = false;
                        smtpMail.Credentials = new NetworkCredential(username, password);
                        smtpMail.EnableSsl = true;
                    }
                }

                smtpMail.Send(mailMsg);

                mailMsg.Dispose();

                Thread.Sleep(2000);
            }
            catch (Exception exEmail)
            {
                // Handle error
                Console.WriteLine("Error sending email: " + exEmail.ToString());
            }
        }

        public void SendEmail(string strSubject, string strMessage, string[] strFiles, string strProcess, DataSet dsEmail)
        {
            try
            {
                MailMessage mailMsg = new MailMessage();
                mailMsg.IsBodyHtml = true;
                mailMsg.Subject = strSubject.Trim();
                mailMsg.Body = strMessage.Trim() + "\n";
                mailMsg.Priority = MailPriority.High;
                mailMsg.IsBodyHtml = true;

                foreach (string file in strFiles)
                {
                    if (!string.IsNullOrEmpty(file) && File.Exists(file))
                    {
                        Attachment msgAttach = new Attachment(file);
                        mailMsg.Attachments.Add(msgAttach);
                    }
                }

                //DataSet dsEmail = new DataSet(); // This line seems unnecessary, consider removing it
                string strSQL = "Select * from tbl_AutoMail_List WHERE ProcessName = @ProcessName";
                SQLHandler sqlHandler = new SQLHandler();
                sqlHandler.CreateParameter(1);
                sqlHandler.SetParameterValues(0, "@ProcessName", SqlDbType.NVarChar, strProcess);

                if (sqlHandler.OpenConnection())
                {
                    if (sqlHandler.FillDataSet(strSQL, out dsEmail, CommandType.Text))
                    {
                        foreach (DataRow row in dsEmail.Tables[0].Rows)
                        {
                            if (Convert.ToBoolean(row["MailTo"]))
                            {
                                mailMsg.To.Add(new MailAddress(row["Email_Address"].ToString().Trim()));
                            }
                            else if (Convert.ToBoolean(row["MailCC"]))
                            {
                                mailMsg.CC.Add(new MailAddress(row["Email_Address"].ToString().Trim()));
                            }
                            else if (Convert.ToBoolean(row["MailFrom"]))
                            {
                                mailMsg.From = new MailAddress(row["Email_Address"].ToString().Trim());
                            }
                        }
                    }
                    sqlHandler.CloseConnection();
                }

                SmtpClient smtpMail = new SmtpClient();
                smtpMail.Host = "atec-mail"; // Replace with actual SMTP host
                smtpMail.Port = 25;
                smtpMail.UseDefaultCredentials = true;
                smtpMail.Credentials = new NetworkCredential("administrator", "trator#$0809");

                smtpMail.Send(mailMsg);

                mailMsg = null;
                smtpMail = null;

                Thread.Sleep(5000);
            }
            catch (Exception exEmail)
            {
                Console.WriteLine("Error sending email: " + exEmail.ToString());
            }
        }

        public void SendEmail(string strSubject, string strMessage, string strFile, string emailFrom, string emailTo, string emailCC)
        {
            try
            {
                MailMessage mailMsg = new MailMessage();
                mailMsg.IsBodyHtml = true;
                mailMsg.Subject = strSubject.Trim();
                mailMsg.Body = strMessage.Trim() + "\n";
                mailMsg.Priority = MailPriority.High;
                mailMsg.IsBodyHtml = true;

                if (!string.IsNullOrEmpty(strFile) && File.Exists(strFile))
                {
                    Attachment msgAttach = new Attachment(strFile);
                    mailMsg.Attachments.Add(msgAttach);
                }

                if (!string.IsNullOrEmpty(emailTo))
                {
                    mailMsg.To.Add(new MailAddress(emailTo));
                }
                if (!string.IsNullOrEmpty(emailCC))
                {
                    mailMsg.CC.Add(new MailAddress(emailCC));
                }
                if (!string.IsNullOrEmpty(emailFrom))
                {
                    mailMsg.From = new MailAddress(emailFrom);
                }

                SmtpClient smtpMail = new SmtpClient();
                smtpMail.Host = "atec-mail"; // Replace with actual SMTP host
                smtpMail.Port = 25;
                smtpMail.UseDefaultCredentials = true;
                smtpMail.Credentials = new NetworkCredential("administrator", "trator#$0809");

                smtpMail.Send(mailMsg);

                mailMsg = null;
                smtpMail = null;

                Thread.Sleep(2000);
            }
            catch (Exception exEmail)
            {
                Console.WriteLine("Error sending email: " + exEmail.ToString());
            }
        }
    }
}
