
function change_status(params){
  var status = $(params).attr('status');
  var plan_code = $(params).attr('plan_code');
  $.post(global_config.lesson_api_url + '/LessonPlan.ashx?action=UPDATE_STATUS&status='+status+'&plan_code='+plan_code).always(function(data){
    if(data=="success"){
        Swal.fire({
            text: "You have successfully updated the status.",
            icon: "success",
            buttonsStyling: !1,
            confirmButtonText: "Ok, got it!",
            customClass: {
                confirmButton: "btn btn-primary"
            }
        }).then((function(t) {
            window.location="index.asp";
        }))
    }else{
        Swal.fire({
            text: data,
            icon: "error",
            buttonsStyling: !1,
            confirmButtonText: "Ok, got it!",
            customClass: {
                confirmButton: "btn btn-primary"
            }
        })
    }
  });
}
function delete_file(plan_code){
  $.post(global_config.lesson_api_url + '/LessonPlan.ashx?action=DELETE&&plan_code='+plan_code).always(function(data){
    if(data=="deleted"){
        Swal.fire({
            text: "You have successfully deleted plan",
            icon: "success",
            buttonsStyling: !1,
            confirmButtonText: "Ok, got it!",
            customClass: {
                confirmButton: "btn btn-primary"
            }
        }).then((function(t) {
           location.reload();
        }))
    }else{
        Swal.fire({
            text: data,
            icon: "error",
            buttonsStyling: !1,
            confirmButtonText: "Ok, got it!",
            customClass: {
                confirmButton: "btn btn-primary"
            }
        })
    }
  });
}
function listgrid(){
  $('#lessons').empty();
  $.get(global_config.lesson_api_url + "/LessonPlan.ashx?action=BY_DATE&criteria="+moment().format('MM/DD/YYYY')).always(function(data){
      var html = '';
      if(data.length ==0){
          html = '<br><h2>No record(s) available<h2>';
      }
      $.each(data,function(k,v){
          html +='<div class="card shadow-sm"  style="background:#E6E8FA; color:#858FE9 ">';
          html +='<div class="card-header">';
          html +='<h3 class="card-title">'+v.COURSE_CODE+'</h3>';
          html +='<div class="card-toolbar">';
          html +='<span class="status"> '+v.STATUS+'</span>';
          html +='<a href="lesson_plan.asp?plan_code='+v.PLAN_CODE+'" class="btn btn-icon btn-color-gray-400 btn-active-color-primary justify-content-end">';
          html +='<span class="svg-icon svg-icon-1">';
          html +='<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none">';
          html +='<rect opacity="0.3" x="2" y="2" width="20" height="20" rx="4" fill="currentColor" />';
          html +='<rect x="11" y="11" width="2.6" height="2.6" rx="1.3" fill="currentColor" />';
          html +='<rect x="15" y="11" width="2.6" height="2.6" rx="1.3" fill="currentColor" />';
          html +='<rect x="7" y="11" width="2.6" height="2.6" rx="1.3" fill="currentColor" />';
          html +='</svg>';
          html +='</span>';
          html +='</a>';
          html +='<a href="#!" onclick="delete_file('+v.PLAN_CODE+')"  class="btn btn-icon btn-color-gray-400 btn-active-color-primary justify-content-end">';
          html +='<span class="fa fa-remove">';
          html +='</span>';
          html +='</a>';
          html +='</div>';
          html +='</div>';
          html +='<div class="card-body">';
          html +='<div class="mb-5">';
          html +='<p class="text-gray-800 fw-normal mb-5">'+moment(v.TIMEIN).format('hh:mm A')+' - '+moment(v.TIMEOUT).format('hh:mm A')+'</p>';
          html +='<div class="d-flex align-items-center mb-5">';
          html +='<div class="col-lg-6">';
          html +='<span class="ttype"> '+v.TYPE+'</span>';
          html +='</div>';
          html +='<div class="col-lg-6">';
          html +='<button type="button" class="btn ttype" style="float:right" status="CANCELED" plan_code="'+v.PLAN_CODE+'" onclick="change_status(this)"> Cancel</button>';
          html +='<button type="button" class="btn ttype" style="float:right" status="COMPLETED" plan_code="'+v.PLAN_CODE+'" onclick="change_status(this)"> Complete</button>'; 
          html +='</div>';
          html +='</div>';
          html +='</div>';
          html +='</div>';
          html +='</div>';
          html +='<br/>';
      });
      $('#lessons').append(html);
  });
}

$(document).ready(function(){
  listgrid();
});