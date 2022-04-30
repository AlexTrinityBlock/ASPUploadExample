using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace UploadFile.Controllers
{
    public class HomeController : Controller
    {

        string connString = "server=127.0.0.1;port=3306;user id=root;password=root;database=mvcdb;charset=utf8;";
        MySqlConnection conn = new MySqlConnection();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        public ActionResult file(HttpPostedFileBase file)
        {
            byte[] imageBytes = null;

            if (file != null && file.ContentLength > 0)
                try
                {
                    string path = Path.Combine(Server.MapPath("~/Content/files"),
                    Path.GetFileName(file.FileName));
                    file.SaveAs(path);
                    ViewBag.Message = "File uploaded successfully";
                    using (BinaryReader theReader = new BinaryReader(file.InputStream))
                    {
                        byte[] thePictureAsBytes = theReader.ReadBytes(file.ContentLength);
                        imageBytes = thePictureAsBytes;
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                }
            else
            {
                ViewBag.Message = "You have not specified a file.";
            }

            //SQL connect
            conn.ConnectionString = connString;
            if (conn.State != ConnectionState.Open)
                conn.Open();
            string sql = @"INSERT INTO images (image , name) VALUES (@image , @name)";
            MySqlCommand cmd = new MySqlCommand(sql, conn);

            cmd.Parameters.Add("@image", MySqlDbType.MediumBlob);
            cmd.Parameters["@image"].Value = imageBytes;

            cmd.Parameters.Add("@name", MySqlDbType.VarChar);
            cmd.Parameters["@name"].Value = file.FileName;

            int index = cmd.ExecuteNonQuery();

            return Content("<script>window.location.replace('../')</script>");

        }

        [HttpGet]
        public ActionResult download(string fileID)
        {
            //SQL connect
            conn.ConnectionString = connString;
            if (conn.State != ConnectionState.Open)
                conn.Open();
            string sql = @"SELECT `image`, `name` FROM `images` WHERE id=@Search";
            MySqlCommand cmd = new MySqlCommand(sql, conn);

            cmd.Parameters.Add("@Search", MySqlDbType.Int64);
            cmd.Parameters["@Search"].Value = fileID;

            DataTable dataTable = new DataTable();
            MySqlDataReader sdr = cmd.ExecuteReader();

            byte[] bytes1 = null;
            string fileName = "";

            while (sdr.Read())
            {
                bytes1 = (byte[])sdr["image"];
                fileName = sdr["name"].ToString();
            }

            //回傳出檔案
            return File(bytes1, "application/unknow", fileName);

        }

        [HttpGet]
        public ActionResult getFileList()
        {
            conn.ConnectionString = connString;
            if (conn.State != ConnectionState.Open)
                conn.Open();
            string sql = @"SELECT `name` FROM `images`";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader sdr = cmd.ExecuteReader();

            string fileName = "";
            List<string> fileList = new List<string>();

            while (sdr.Read())
            {
                fileList.Add(sdr["name"].ToString());
            }

            var jsonData=JsonConvert.SerializeObject(fileList);

            var resultData = new { data = jsonData};
            return Json(resultData, JsonRequestBehavior.AllowGet);
        }
    }
}