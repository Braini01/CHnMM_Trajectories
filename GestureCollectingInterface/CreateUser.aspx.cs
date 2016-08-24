using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using LfS.GestureDatabase;

namespace GestureCollectingInterface
{
    public partial class CreateUser : System.Web.UI.Page
    {
        /*
        protected bool validateAge()
        {
            int age;
            if (int.TryParse(tbAge.Text, out age))
                if (age >= 0 && age <= 150) return true;

            return false;
        }
        */
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnCreateUser_Click(object sender, EventArgs e)
        {
            bool userAdded = GestureDatabase.addUser(tbUsername.Text, tbName.Text);
            if (!userAdded) throw new ApplicationException("User wasn't added. User already exists?");
            Response.Redirect("InputData.aspx");
        }
    }
}
