using MaradTestDBL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace NewsToEmailWebApp.MaradTest
{
    public partial class Test : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {            
            var context = new MaradTestDBL.MaradDBEntities();
            if (!Page.IsPostBack)
            {
                int userId = 0;
                List<string> category = new List<string>();
                string level = "";
                string testType = "";
                try
                {
                    testType = Page.Request.Params["type"];
                    int.TryParse(Page.Request.Params["id"],out userId);
                    category = Page.Request.Params["category"].Trim(';').Split(';').ToList();
                    category.RemoveAll(c => string.IsNullOrWhiteSpace(c));
                    level = Page.Request.Params["level"];
                }
                catch (Exception)
                {
                }                
                var questionsFiltered = (from q in context.Questions
                                         where (category.Count > 0 & category.Contains(q.category)) & q.level.Contains(level)
                                         select q).ToList();
                ViewState["pageType"] = "";
                switch (testType)
                {
                    case "Original":
                        Random rnd = new Random();                     
                        for (int i = 0; i < 60; i++)
                        {
                            var id = rnd.Next(0, questionsFiltered.Count - 1);
                            ViewState["questions"] += questionsFiltered[id].Id + ";";
                            questionsFiltered.RemoveAt(id);
                        }
                        break;
                    case "All":
                        foreach (var item in questionsFiltered)
                        {
                            ViewState["questions"] += item.Id + ";";                           
                        }
                        break;
                    case "Bad Questions Only":
                        List<Questions> questionsBadAU = new List<Questions>();
                        for (int i = 0; i < questionsFiltered.Count; i++)
                        {
                            var q = questionsFiltered[i];
                            var stats = q.Statistics.Where(s => s.userId == userId).FirstOrDefault();
                            if (stats != null)
                            {
                                try
                                {
                                    double correctRatio = (double)stats.answeredCorrect / (double)stats.answeredTotal;
                                    if (correctRatio < 0.7)
                                    {
                                        questionsBadAU.Add(q);
                                    }
                                }
                                catch (Exception)
                                {
                                    questionsBadAU.Add(q);
                                }
                            }
                        }
                        foreach (var item in questionsBadAU)
                        {
                            ViewState["questions"] += item.Id + ";";
                        }
                        break;
                    case "Bad and Unviewed Questions":
                        List<Questions> questionsBad = new List<Questions>();
                        for (int i = 0; i < questionsFiltered.Count; i++)
                        {
                            var q = questionsFiltered[i];
                            var stats = q.Statistics.Where(s => s.userId == userId).FirstOrDefault();
                            if (stats == null)
                            {
                                questionsBad.Add(q);
                            }
                            else
                            {
                                try
                                {
                                    double correctRatio = (double)stats.answeredCorrect / (double)stats.answeredTotal;
                                    if (correctRatio < 0.70)
                                    {
                                        questionsBad.Add(q);
                                    }
                                }
                                catch (Exception)
                                {
                                    questionsBad.Add(q);
                                }
                            }
                        }
                        foreach (var item in questionsBad)
                        {
                            ViewState["questions"] += item.Id + ";";
                        }
                        break;
                    case "Unviewed Questions":
                        List<Questions> questionsUnv = new List<Questions>();
                        for (int i = 0; i < questionsFiltered.Count; i++)
                        {
                            var q = questionsFiltered[i];
                            var stats = q.Statistics.Where(s => s.userId == userId).FirstOrDefault();
                            if (stats == null)
                            {
                                questionsUnv.Add(q);
                            }
                        }
                        foreach (var item in questionsUnv)
                        {
                            ViewState["questions"] += item.Id + ";";
                        }
                        break;
                    case "Bad Questions Review":
                        List<Questions> questionsBadRev = new List<Questions>();
                        for (int i = 0; i < questionsFiltered.Count; i++)
                        {
                            var q = questionsFiltered[i];
                            var stats = q.Statistics.Where(s => s.userId == userId).FirstOrDefault();
                            if (stats != null)
                            {
                                try
                                {
                                    double correctRatio = (double)stats.answeredCorrect / (double)stats.answeredTotal;
                                    if (correctRatio < 0.8)
                                    {
                                        questionsBadRev.Add(q);
                                    }
                                }
                                catch (Exception)
                                {
                                    questionsBadRev.Add(q);
                                }
                            }
                        }
                        var wrong = "";
                        foreach (var item in questionsBadRev)
                        {
                            wrong += item.Id + ";";
                        }
                        Response.Redirect("~/MaradTest/Test.aspx?id=" + userId + "&type=" + wrong);
                        break;
                    default:
                        ViewState["pageType"] = "answers";
                        ViewState["questions"] = testType;
                        break;
                }
                var user = context.Users.Find(userId);
                if (user!=null)
                {
                    if (user.name == "didoeddy")
                    {
                        qIdLabel.Visible = true;
                    }
                }
                ViewState["totalQuestions"] = ViewState["questions"].ToString().Trim(';').Split(';').Length;
                ViewState["currentQuestionNumber"] = 1;
                ViewState["correctAnswered"] = 0;
                ViewState["wrongQuestions"] = "";
            }
        }

        private void BindImage(Questions currentQ)
        {
            if (!string.IsNullOrWhiteSpace(Image.ImageUrl))
            {
                var oldpath = Request.MapPath(Image.ImageUrl);
                File.Delete(oldpath);
            }
            if (currentQ.questionImage == null)
            {
                Image.Visible = false;
            }
            else
            {                
                var imageName = "~/Images/questionImage" + currentQ.Id + ".png";
                var path = Request.MapPath(imageName);
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                
                        var byteList = new List<byte>(currentQ.questionImage);
                        byteList.TrimExcess();
                        fs.Write(byteList.ToArray(), 0, byteList.Count);
                        Image.ImageUrl = imageName;
                        Image.Visible = true;
                }
            }
        }

        protected void AnswersRBL_DataBound(object sender, EventArgs e)
        {
            string pageType = "";
            try
            {
                pageType = ViewState["pageType"].ToString();
            }
            catch (Exception)
            {
            }
            if (pageType=="answers")
            {
                int qId;
                int.TryParse(qIdLabel.Text,out qId);
                var context = new MaradDBEntities();
                var currentQ = context.Questions.Find(qId);
                if (currentQ!=null)
                {
                    foreach (ListItem item in AnswersRBL.Items)
                    {
                        if (item.Text== currentQ.correctAnswer)
                        {
                            item.Selected = true;
                            continue;
                        }
                    }
                }
            }
        }

        protected void UpdateQuestionPanel_Load(object sender, EventArgs e)
        {
            string pageType = "";
            try
            {
                pageType = ViewState["pageType"].ToString();
            }
            catch (Exception)
            {
            }
            var context = new MaradDBEntities();
            if (pageType != "answers")
            {
                int qId;
                int.TryParse(qIdLabel.Text, out qId);
                var current = context.Questions.Find(qId);
                int userId;
                int.TryParse(Page.Request.Params["id"], out userId);
                if (current != null)
                {
                    if (AnswersRBL.SelectedItem != null)
                    {
                        var answer = AnswersRBL.SelectedItem.Text;
                        var stats = context.Statistics.FirstOrDefault(s => s.questionId == current.Id & s.userId == userId);
                        if (stats == null)
                        {
                            stats = new Statistics() { questionId = current.Id, userId = userId, answeredTotal = 1, answeredCorrect = 0 };
                            context.Statistics.Add(stats);
                        }
                        else
                        {
                            stats.answeredTotal++;
                        }
                        if (ViewState["LastWrong"] == "true")
                        {
                            return;
                        }
                        if (current.correctAnswer.ToUpper().Trim() == answer.ToUpper().Trim())
                        {
                            stats.answeredCorrect++;
                            ViewState["correctAnswered"] = int.Parse(ViewState["correctAnswered"].ToString()) + 1;
                            context.SaveChanges();
                        }
                        else
                        {
                            ViewState["wrongQuestions"] = ViewState["wrongQuestions"] + qId.ToString() + ";";
                            context.SaveChanges();
                            ViewState["LastWrong"] = "true";
                            foreach (ListItem item in AnswersRBL.Items)
                            {
                                if (item.Text.ToUpper().Trim() == current.correctAnswer.ToUpper().Trim())
                                {
                                    item.Attributes.Add("style", "color:green");
                                    continue;
                                }
                            }
                        }
                        if (ViewState["LastWrong"] == "true")
                        {
                            return;
                        }
                    }
                }
            }
            var questionsIdList = ViewState["questions"].ToString().Trim(';').Split(';').ToList();
            questionsIdList.RemoveAll(q => string.IsNullOrWhiteSpace(q));
            if (questionsIdList.Count < 1)
            {
                EndTest(pageType);
            }
            var qid = int.Parse(questionsIdList[0]);
            var currentQ = context.Questions.Find(qid);
            questionLabel.Text = currentQ.questionText;
            statusLabel.Text = string.Format("{0} of {1}", ViewState["currentQuestionNumber"], ViewState["totalQuestions"]);
            qIdLabel.Text = currentQ.Id.ToString();
            Correct.Attributes.Add("alt", currentQ.correctAnswer);
            BindImage(currentQ);
            AnswersRBL.DataSource = currentQ.answers.Trim(';').Split(';').ToList();
            AnswersRBL.DataBind();
            questionsIdList.RemoveAt(0);
            string left = "";
            foreach (var bad in questionsIdList)
            {
                left += bad + ";";
            }
            ViewState["questions"] = left;

            ViewState["currentQuestionNumber"] = int.Parse(ViewState["currentQuestionNumber"].ToString()) + 1;
        }

        private void EndTest(string pageType)
        {
            var wrongQuestions = ViewState["wrongQuestions"].ToString().Trim(';').Split(';').ToList();
            wrongQuestions.RemoveAll(q => string.IsNullOrWhiteSpace(q));
            int userId = 0;
            int.TryParse(Page.Request.Params["id"], out userId);
            if (wrongQuestions.Count > 0)
            {
                Response.Redirect("~/MaradTest/TestResult.aspx?id="+ userId + "&wrong=" + Server.UrlEncode(ViewState["wrongQuestions"].ToString()) + "&total=" + Server.UrlEncode(ViewState["totalQuestions"].ToString()));
            }
            else
            {
                if (pageType == "answers")
                {
                    Response.Redirect("~/MaradTest/MaradTest.aspx");
                }
                else
                {
                    Response.Redirect("~/MaradTest/TestResult.aspx");
                }
            }
        }

        protected void Image_Load(object sender, EventArgs e)
        {
            var image = (sender as Image);
        }

        protected void finishBtn_Click(object sender, EventArgs e)
        {
            ViewState["questions"] = "";
            ViewState["totalQuestions"] = int.Parse(ViewState["currentQuestionNumber"].ToString())-3;
            string pageType = "";
            try
            {
                pageType = ViewState["pageType"].ToString();
            }
            catch (Exception)
            {
            }
            EndTest(pageType);
        }

        protected void nextBtn_Click(object sender, EventArgs e)
        {
            if (ViewState["LastWrong"] == "true")
            {
                var context = new MaradDBEntities();
                ViewState["LastWrong"] = "false";
                var questionsIdList = ViewState["questions"].ToString().Trim(';').Split(';').ToList();
                questionsIdList.RemoveAll(q => string.IsNullOrWhiteSpace(q));
                var pageType = ViewState["pageType"].ToString();
                if (questionsIdList.Count < 1)
                {
                    EndTest(pageType);
                }
                var qid = int.Parse(questionsIdList[0]);
                var currentQ = context.Questions.Find(qid);
                questionLabel.Text = currentQ.questionText;
                statusLabel.Text = string.Format("{0} of {1}", ViewState["currentQuestionNumber"], ViewState["totalQuestions"]);
                qIdLabel.Text = currentQ.Id.ToString();
                Correct.Attributes.Add("alt", currentQ.correctAnswer);
                BindImage(currentQ);
                AnswersRBL.DataSource = currentQ.answers.Trim(';').Split(';').ToList();
                AnswersRBL.DataBind();
                ViewState["questions"] = ViewState["questions"].ToString().Replace(qIdLabel.Text + ";", "");
                ViewState["currentQuestionNumber"] = int.Parse(ViewState["currentQuestionNumber"].ToString()) + 1;
            }
            
        }
    }
}