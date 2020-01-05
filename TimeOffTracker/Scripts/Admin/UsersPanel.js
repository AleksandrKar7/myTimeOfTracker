
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

function OnComplete(currentPage, TotalPage) {
    $("#pagination").load("Pagination?page=" + currentPage + "&count=" + TotalPage);
}