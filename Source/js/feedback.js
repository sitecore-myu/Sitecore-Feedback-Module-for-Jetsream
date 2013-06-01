$(document).ready(function () {
    
    var countHistorySave = 10;
    
    var sortJson = function (data, key, way) {
        return data.sort(function (a, b) {
            var x = a[key];
            var y = b[key];
            if (way === 'asc') {
                return ((x < y) ? -1 : ((x > y) ? 1 : 0));
            }
            if (way === 'desc') {
                return ((x > y) ? -1 : ((x < y) ? 1 : 0));
            }
        });
    };

    try {
        
        var userPages = $.cookie("feedback_UserVisitPages");
        var lastVisitPage = $.cookie("lastVisitPage");
        if (userPages == null || userPages == '' || userPages == undefined)
            userPages = { "Pages": [] };
        else 
            userPages = JSON.parse(userPages);
        
        if (userPages.Pages.length + 1 <= countHistorySave) {
            if (lastVisitPage != window.location.href) {
                userPages.Pages.push({ id: userPages.Pages.length + 1, url: window.location.href });
                lastVisitPage = window.location.href;
                userPages.Pages = sortJson(userPages.Pages, 'id', 'desc');
            }

        } else {
            if (lastVisitPage != window.location.href) 
            {
                userPages.Pages = sortJson(userPages.Pages, 'id', 'desc');
                userPages.Pages.pop();
                userPages.Pages = sortJson(userPages.Pages, 'id', 'asc');
                for (var i = 0; i < userPages.Pages.length; i++)
                    userPages.Pages[i].id = i + 1;

                userPages.Pages = sortJson(userPages.Pages, 'id', 'desc');
                userPages.Pages.push({ id: userPages.Pages.length + 1, url: window.location.href });
                lastVisitPage = window.location.href;
                userPages.Pages = sortJson(userPages.Pages, 'id', 'desc');
            }
        }
        $.cookie("feedback_UserVisitPages", JSON.stringify(userPages), { path: '/' });
        $.cookie("lastVisitPage", lastVisitPage, { path: '/' });
    } catch (e) {
        //alert(e);
    }

});