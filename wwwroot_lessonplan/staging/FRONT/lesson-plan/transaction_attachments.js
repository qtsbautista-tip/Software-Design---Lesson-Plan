

function listgrid(){
	$('#grid').empty();
	var folder = window.params.module_code;
	var refno = window.params.refno;
	var crudServiceBaseUrl = global_config.systemapi_url;
    var dataSource = new kendo.data.DataSource({
        transport: {
            read: {
                url: crudServiceBaseUrl + "/GetAllFiles.ashx?module_code=" + folder + "&refno=" + refno,
                dataType: "json"
            },
            parameterMap: function (options, operation) {
                if (operation !== "read" && options.models) {
                    return { models: kendo.stringify(options.models) };
                }
            }
        },
        error: function (e) {
            if (e.xhr.responseText.startsWith("error:")) {
                alert(e.xhr.responseText);
            }
        },
        batch: true,
        //           pageSize: 20,
        schema: {
            model: {
                id: "FILENAME",
                fields: {
                    FILENAME: { validation: { required: true} }
                }
            }
        }
    });
	$("#grid").kendoGrid({
		dataSource: dataSource,
		scrollable: true,
		height: 400,
		toolbar: [
			{
				name: "attachments",
				template: "<button type='button' class='k-button' onclick='upload_files()'>Upload Attachment</button>"
			},
			{
				name: "refresh",
				template: "<button type='button' class='k-button' onclick='listgrid()'>Refresh</button>"
			}
		],
		columns: [{
				field: "file_name",
				title: "Filename",
				template: "<span style='color:blue;cursor: pointer;' onclick='openfile(this)' dataid='#=id#' fileext='#=file_ext#' filename='#=file_name#'>#= file_name #</span>",
				width: "80px"
			},
			{
				field: "upload_date",
				title: "Date",
				width: "20px",
				hidden:true,
				template: "#= kendo.toString(kendo.parseDate(upload_date, 'yyyy-MM-dd'), 'MM/dd/yyyy') #"
			},
			{
				name: "deleteitem",
				template: "<button class='' type='button' onclick='download_files(this)' dataid='#=id#' fileext='#=file_ext#' filename='#=file_name#' title='Download'><i class='fa fa-download'></i></button> <button type='button' style='width:10px'  id='${ uid }' onclick='deletefile(this)' dataid='#=id#' fileext='#=file_ext#' filename='#=file_name#' title='Delete'><i class='fa fa-trash'></i></button>",
				width: "20px"
			},
			{
				field: "refno",
				hidden: true
			},
		],
	});
}
// open the files uploaded
function openfile(p) {
	var id = $(p).attr('dataid');
	var file_ext = $(p).attr('fileext');
	var filename = $(p).attr('filename');
	var module_code = window.params.module_code;
	var refno = window.params.refno;
	url = global_config.files_upload_url +module_code+'/'+ refno+'/' + filename;
	dynamicWin("open_uploaded_file", "Open File", url, 200, "max", true);
}
// download files uploaded
function download_files(p) {
	var id = $(p).attr('dataid');
	var file_ext = $(p).attr('fileext');
	var filename = $(p).attr('filename');
	var refno = window.params.refno;
	var module_code = window.params.module_code;
	var refno = window.params.refno;
	url = global_config.files_upload_url +module_code+'/'+ refno+'/' + filename;
	window.open(url, '_blank');
}


// delete file uploaded
function deletefile(p) {
	bootbox.confirm('Delete this file?', function(ans) {
		if (ans) {
			var id = $(p).attr('dataid');
			var file_ext = $(p).attr('fileext');
			var file_name = $(p).attr('filename');
			var module_code = window.params.module_code;
			var refno = window.params.refno;
			var mysession = deserialize(Cookies.get(global_config.cookie));
			var url = global_config.systemapi_url + '/DeleteFile.ashx?module_code=' + module_code + '&refno=' + refno + '&file_name=' + file_name+'&username='+mysession.user_name;
			$.get(url).always(function() {
				bootbox.alert('File deleted!',function(){
					location.reload();
				});
			});
		}
	});

}

//upload file
function upload_files() {
	var global_module_code = window.params.module_code;
	var refno = window.params.refno;
	if (refno == '') {
		bootbox.alert('refno is required');
		return;
	}
	var mysession = deserialize(Cookies.get(global_config.cookie));
	var url = global_config.main_url+'/fileupload.asp?multiple=true&module_code=' + global_module_code + '&refno=' + refno+'&username='+mysession.user_name;
	dynamicWin("upload_files", "Upload Files", url, 200, "max", true);
}

// Main entry (Form Load)
$(document).ready(function() {
	var module_code = window.params.module_code;
	var refno = window.params.refno;
	$('#module_code').text(module_code);
	$('#refno').text(refno);
	listgrid();
});
