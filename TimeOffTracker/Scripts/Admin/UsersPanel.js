
$(document).ready(
    //Авто-загрузка первой страницы
    function () {
        $("#results").load("GetPartOfUsers");
        $("#pagination").load("Pagination");
    //Скрипт для кнопки Show More
    document.body.onclick = function (e) {
        e = e || event;
        var target = e.target || e.srcElement;
        if (target.id == 'next') {

            pageNum++;
            page.value = pageNum;

        }
    }
    });

var sortLock = false;

var FullName, Email, EDate, Roles;
function SetSortSatuses(fullName, email, eDate, roles) {
    FullName = fullName;
    Email = email;
    EDate = eDate;
    Roles = roles;
    SetIcon(fullName, email, eDate, roles);
}

function ChangeSort(fullName, email, eDate, roles)
{
    if (sortLock == false) {
        sortLock = true;
        if (fullName != null) {
            FullName = FullName == null ? fullName : !FullName;
        } else {
            FullName = null;
        }

        if (email != null) {
            Email = Email == null ? email : !Email;
        } else {
            Email = null;
        }

        if (eDate != null) {
            EDate = EDate == null ? eDate : !EDate;
        } else {
            EDate = null;
        }

        if (roles != null) {
            Roles = Roles == null ? roles : !Roles;
        } else {
            Roles = null;
        }
        $.ajax(
            {
                url: ("UpdateSortInfo?fullName=" + FullName + "&email=" + Email + "&eDate=" + EDate + "&roles=" + Roles)
                , beforeSend: function () { SetIcon(FullName, Email, EDate, Roles) }
                , success: function () {
                    RefreshInfo();
                    
                }
                
            }
            );
    }
}

function RefreshInfo() {
    $("#results").load(("GetPartOfUsers?page=" + 1),
        function () {
            sortLock = false;
            SetIcon(FullName, Email, EDate, Roles);
        });
    $("#pagination").load("Pagination");    
}
var i = 0;

function SetIcon(fullName, email, eDate, roles) {
    if (fullName != null) {
        if (sortLock == true) {
            $("#fullNameIcon").text("‎↺");
        }
        else if (FullName == true) {
            $("#fullNameIcon").text("‎▲");
        } else {
            $("#fullNameIcon").text("‎▼");
        }
    } else {
        $("#fullNameIcon").text("‎");
    }

    if (email != null) {
        if (sortLock == true) {
            $("#emailIcon").text("‎↺");
        }
        else if (email == true) {
            $("#emailIcon").text("‎▲");
        } else {
            $("#emailIcon").text("‎▼");
        }
    } else {
        $("#emailIcon").text("‎");
    }

    if (eDate != null) {
        if (sortLock == true) {
            $("#eDateIcon").text("‎↺");
        }
        else if (eDate == true) {
            $("#eDateIcon").text("‎▲");
        } else {
            $("#eDateIcon").text("‎▼");
        }
    } else {
        $("#eDateIcon").text("‎");
    }

    if (roles != null) {
        if (sortLock == true) {
            $("#rolesIcon").text("‎↺");
        }
        else if (roles == true) {
            $("#rolesIcon").text("‎▲");
        } else {
            $("#rolesIcon").text("‎▼");
        }
    } else {
        $("#rolesIcon").text("‎");
    }
}

function OnComplete(currentPage, TotalPage) {
    $("#pagination").load("Pagination?page=" + currentPage + "&count=" + TotalPage);
}