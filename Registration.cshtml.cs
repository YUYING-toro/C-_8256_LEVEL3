using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AcademicManagement;
using Microsoft.AspNetCore.Http;

namespace Lab2.Pages  // 位置 model or pages
{
    public class RegistrationModel : PageModel
    {
        public string slesctedId { get; set; }
        [BindProperty]
        public string sId { get; set; }
        public string orderbySavetohtml { get; set; }

        public string stGrade { get; set; }
        public bool showNoSelectCourse = false;
        public bool showNoSt = false;
        public bool showOptions = false;

        public bool errorRepeatedSelectedClass = false;
        public bool che;    //RecordsByStudentId(sId).Find(a => a.CourseCode == selesctedClass) != null
        public string test { get; set; }
        public string gradeCode {get; set;}
        public  List<AcademicRecord> selectedSortedCourse { get; set; }  //cshtml 

        //session variable
        public string getSessionSid { get; set; }
        public List<AcademicRecord> getSessionCourse { get; set; }  //according session have sid to get privous course 
        public List<string> sortedCodeList { get; set; }

        public void OnGet(string orderby)
        {

            //get privious sid
            if (orderby != null)  //have para > have before sid course > T get record and sort > F all course
            {
                orderbySavetohtml = orderby;
                HttpContext.Session.SetString("orderby", orderby);
                getSessionSid = HttpContext.Session.GetString("SelectedSid");

                if (getSessionSid != null && getSessionSid != "-1")  //old st  不是getSessionSid == sId
                {
                    sId = getSessionSid;
                    getSessionCourse = DataAccess.GetAcademicRecordsByStudentId(getSessionSid);
                    if (getSessionCourse.Count() > 0) 
                    {
                        if (orderby == "code")
                        {
                            getSessionCourse.Sort((x, y) => x.CourseCode.CompareTo(y.CourseCode));
                            selectedSortedCourse = getSessionCourse;
                        }
                        else if (orderby == "grade")
                        {
                            getSessionCourse.Sort((x, y) => x.Grade.CompareTo(y.Grade));
                            selectedSortedCourse = getSessionCourse;
                        }
                        else if (orderby == "title") 
                        {
                            List<string> saveTitleToSort = new List<string>();
                            //List<string> saveCode_accordSortedTile = new List<string>();

                            foreach (var item in DataAccess.GetAllCourses())
                            {
                                //save title
                                saveTitleToSort.Add(new string(item.CourseTitle)); //all title list
                            }
                            saveTitleToSort.Sort((s, y) => s.CompareTo(y));  //sort alltitle list
                            var collectSortedCode = new List<string>();
                            foreach(string i in saveTitleToSort)
                            {
                                
                            //sorted title list => sorted code list
                            string sortedCode_accordSortedTitle = DataAccess.GetAllCourses().Find(x => x.CourseTitle == i).CourseCode;

                                //sortedCodeList.Add(new string(sortedCode_accordSortedTitle));
                                collectSortedCode.Add(sortedCode_accordSortedTitle);    
                            }
                            var ListStHasThisCode = new List<string>();
                            //this st has this class code
                            foreach (string i in collectSortedCode) 
                            {

                                var chechStHasThisCode = DataAccess.GetAcademicRecordsByStudentId(sId).FirstOrDefault(x => x.CourseCode == i);
                                if (chechStHasThisCode != null)   //avoid has this course but st no seleceted is Null
                                {
                                    string code_chechStHasThisCode = DataAccess.GetAcademicRecordsByStudentId(sId).Find(x => x.CourseCode == i).CourseCode;
                                    ListStHasThisCode.Add(code_chechStHasThisCode);
                                }
                            }
                            sortedCodeList = ListStHasThisCode;  // sorted code and st has selected code
                        }
                    }

                }
            }
            DataAccess.GetAllStudents(); //action > get all st
        }

        public void OnPostStudentSelected()
        {
            if (sId == null)  // not != '-1'
            {
                showNoSt = true;
            }
            else if (DataAccess.GetAcademicRecordsByStudentId(sId).Count == 0)
            {
                showOptions = true;
                HttpContext.Session.SetString("SelectedSid",sId);  // storage sid
            }
        }
        public void OnPostRegister() {
            string selesctedClass = Request.Form["selesctedClass"];
            if (selesctedClass == null)
            {
                showNoSelectCourse = true;
                showOptions = true;
            }
            else {
                che = DataAccess.GetAcademicRecordsByStudentId(sId).Find(a => a.CourseCode == selesctedClass) != null;  //有找到東西
                if (!che)
                {
                    List<string> selectedCoursesList = new();
                    selectedCoursesList = selesctedClass.Split(",").ToList();
                    foreach (string selecCourse in selectedCoursesList) {
                        AcademicRecord course = new();      //plugIn 
                        course.CourseCode = selecCourse;
                        course.StudentId = sId;
                        DataAccess.AddAcademicRecord(course);
                    }
                }
                else
                {
                    errorRepeatedSelectedClass = true;
                }
            }
        }
        
        public void OnPostBtnGrade() {

            //get grade
            stGrade = Request.Form["grade"];  //stringsss
            List<string> gradesList = new();
            gradesList = stGrade.Split(",").ToList();
            List<double> gradesListNum = new();
            foreach (string i in gradesList) { gradesListNum.Add(double.Parse(i)); }

            int ind = 0;       
            foreach (var item in DataAccess.GetAcademicRecordsByStudentId(sId))
            {                
                item.Grade = gradesListNum[ind];  //init -100
                ind += 1;
            }

        }
    }

}
