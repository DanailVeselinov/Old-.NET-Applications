<%@ Application Codebehind="Global.asax.cs" Inherits="NewsToEmailWebApp.Global" Language="C#" %>
 <script runat="server" >        
 void Application_PreRequestHandlerExecute        (object sender, EventArgs e)    
 {  	HttpContext context = HttpContext.Current;
		// Check we are actually in a webforms page.        
		Page page = context.Handler as Page;
        if (page != null)        
		{            
			// Use the authenticated user if one is available,
            // so as the user key does not expire over
            // application recycles.
            if (context.Request.IsAuthenticated)
            {
				page.ViewStateUserKey = context.User.Identity.Name;
			}
            else
            {
				page.ViewStateUserKey = context.Session.SessionID;
			}
		}
	}
</script>  