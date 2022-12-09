function listgrid(){
    $('#courses').empty();
    $.get(global_config.lesson_api_url + "/Courses.ashx?action=ALL").always(function(data){
        var html = '';
        $.each(data,function(k,v){
            
		    html +='<div class="col-md-6 col-lg-4 col-xl-3">';
            html +='<div class="card h-100" style="background-color:'+v.BGCOLOR+'">';
            html +='<div class="card-body d-flex justify-content-center text-center flex-column p-8">';
            html +='<a href="lesson_plan.asp?code='+v.COURSE_CODE+'" class="text-gray-800 text-hover-primary d-flex flex-column">';
            html +='<div class="symbol symbol-60px mb-5">';
            html +='<img src="resources/courses/'+v.ICON+'" class="theme-light-show" alt=""   style="background-color:'+v.COLOR+'">';
            html +='</div>';
            html +='<div class="fs-5 fw-bold mb-2">'+v.COURSE_CODE+'</div>';
            html +='<div class="fs-5 fw-bold mb-2">'+v.COURSE_DESC+'</div>';
            html +='</a>';
            html +='</div>';
            html +='</div>';
            html +='</div>';
        });
        $('#courses').append(html);
    });
}

$(document).ready(function(){
    listgrid();
});