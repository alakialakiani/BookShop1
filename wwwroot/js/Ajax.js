$(function () {
    var placeholder = $("#modal-placeholder");

    $(document).on('click','button[data-toggle="ajax-modal"]',function () {
        var url = $(this).data('url');
        $.ajax({
            url: url,
            beforeSend: function () { ShowLoading(); },
            complete: function () { window.loading_screen.finish(); },
            error: function () {
                ShowSweetAlert();
            }
        }).done(function (result) {
            placeholder.html(result);
            placeholder.find('.modal').modal('show');
        });
    });

    placeholder.on('click', 'button[data-save="modal"]', function () {
        ShowLoading();
        var form = $(this).parents(".modal").find('form');
        var actionUrl = form.attr('action');
        var dataToSend = new FormData(form.get(0));

        $.ajax({
            url: actionUrl, type: "post", data: dataToSend, processData: false, contentType: false, error: function () {
                ShowSweetAlert();
            }}).done(function (data) {
            var newBody = $(".modal-body", data);
            placeholder.find(".modal-body").replaceWith(newBody);

            var IsValid = newBody.find("input[name='IsValid']").val() === "True";
            if (IsValid) {
                var notificationPlaceholder = $("#notification");
                var notificationUrl = notificationPlaceholder.data('url');
                $.ajax({ url: notificationUrl, error: function () { ShowSweetAlert(); } }).done(function (notification) {
                    notificationPlaceholder.html(notification);
                });

                var tableElement = $("#myTable");
                var tableUrl = tableElement.data('url');
                $.ajax({ url: tableUrl, error: function () { ShowSweetAlert(); } }).done(function (table) {
                    $("#tableContent").html(table);
                });

                placeholder.find(".modal").modal('hide');
            }
        });

        window.loading_screen.finish(); 
    });
});

function ShowSweetAlert() {
    Swal.fire({
        type: 'error',
        title: 'خطایی رخ داده است !!!',
        text: 'لطفا تا برطرف شدن خطا شکیبا باشید.',
        confirmButtonText: 'بستن'
    });
}

function ShowLoading() {
    window.loading_screen = window.pleaseWait({
        backgroundColor: 'rgba(255, 255, 255, 0.63)',
        loadingHtml: "<div class='sk-spinner sk-spinner-wave'><img src='/images/loading.gif' width='128' /></div><p class='loading-message'>لطفا صبر کنید...</p>"
    });
}


