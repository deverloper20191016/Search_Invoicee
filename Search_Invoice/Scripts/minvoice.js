function PrintInvoice(url, model, textTitle, actionFile) {

    bootbox.dialog({
        title: textTitle,
        message: "<p class='text-center' ><i style='font-size:350%;' class='fa fa-spin fa-spinner'></i></p>",
        buttons: {
            cancel: {
                label: '<i class="fa fa-times"></i> Hủy',
                className: 'btn-danger',
                callback: function () {
                    clearTimeout(interVal);
                    bootbox.hideAll();
                }
            }
        }
    });
    var interVal = setTimeout(function () {
        var xhr = new XMLHttpRequest();
        xhr.open('POST', url, true);
        xhr.setRequestHeader("Content-type", "application/json;charset=UTF-8");
        xhr.responseType = 'arraybuffer';
        xhr.send(model);
        xhr.onload = function () {
            if (this.status === 200) {
                debugger;
                var filename = "";
                var disposition = xhr.getResponseHeader('Content-Disposition');
                if (disposition && disposition.indexOf('attachment') !== -1) {
                    var filenameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;
                    var matches = filenameRegex.exec(disposition);
                    if (matches != null && matches[1]) filename = matches[1].replace(/['"]/g, '');
                }
                var type = xhr.getResponseHeader('Content-Type');
                var blob = typeof File === 'function' ? new File([this.response], filename, {
                    type: type
                }) : new Blob([this.response], {
                    type: type
                });

                if (typeof window.navigator.msSaveBlob !== 'undefined') {
                    window.navigator.msSaveBlob(blob, filename);
                } else {
                    var URL = window.URL || window.webkitURL;
                    var downloadUrl = URL.createObjectURL(blob);
                    var a = document.createElement("a");
                    if (actionFile === "DOWNLOAD") {
                        filename = $("#fileName").val();
                        document.body.appendChild(a);
                        a.href = downloadUrl;
                        a.download = filename;
                        a.click();
                        window.URL.revokeObjectURL(downloadUrl);
                        document.body.removeChild(a);
                        bootbox.hideAll();
                    } else {
                        if (filename) {
                            if (typeof a.download === 'undefined') {
                                bootbox.hideAll();
                                clearTimeout(interVal);
                                var newWindow = window.open('/');
                                newWindow.onload = () => {
                                    newWindow.location = downloadUrl;
                                };
                            } else {
                                bootbox.hideAll();
                                clearTimeout(interVal);
                                var newWindow = window.open('/');
                                newWindow.onload = () => {
                                    newWindow.location = downloadUrl;
                                };
                            }
                            bootbox.hideAll();
                            clearTimeout(interVal);
                            var newWindow = window.open('/');
                            newWindow.onload = () => {
                                newWindow.location = downloadUrl;
                            };
                        } else {
                            bootbox.hideAll();
                            clearTimeout(interVal);
                            var newWindow = window.open('/');
                            newWindow.onload = () => {
                                newWindow.location = downloadUrl;
                            };
                        }
                    }
                }
            } else {
                bootbox.hideAll();
                clearTimeout(interVal);
                bootbox.alert("Có lỗi xảy ra !");
            }
        };
    }, 1000);
};

function PrintInvoicePDF() {
    var mst = $("#mst_html").val();
    var sbm = $("#sobaomat_html").val();
    var type = "PDF";

    var model = '{ "sobaomat": "' + sbm + '", "masothue": "' + mst + '","type":"' + type + '" }';
    var title = "Đang in hóa đơn ...";
    var url = "/Tracuu2/PrintInvoice";
    var actionFile = "VIEW";
    PrintInvoice(url, model, title, actionFile);
};

function PrintInvoiceChuyenDoiPDF() {
    var mst = $("#mst_html").val();
    var sbm = $("#sobaomat_html").val();
    var inchuyendoi = 1;
    var type = "PDF";

    var model = '{ "sobaomat": "' + sbm + '", "masothue": "' + mst + '","type":"' + type + '", "inchuyendoi":"' + inchuyendoi + '" }';
    var title = "Đang in hóa đơn ...";
    var url = "/Tracuu2/PrintInvoice";
    var actionFile = "VIEW";
    PrintInvoice(url, model, title, actionFile);
};

function ExportZipXML() {
    var mst = $("#mst_html").val();
    var sbm = $("#sobaomat_html").val();
    var model = '{ "sobaomat": "' + sbm + '", "masothue": "' + mst + '" }';
    var title = "Đang Export hóa đơn File .Zip ...";
    var url = "/Tracuu2/ExportZipFileXML";
    var actionFile = "DOWNLOAD";
    PrintInvoice(url, model, title, actionFile);
};

function DownloadInvoicePDF() {
    var mst = $("#mst_html").val();
    var sbm = $("#sobaomat_html").val();
    var type = "PDF";

    var model = '{ "sobaomat": "' + sbm + '", "masothue": "' + mst + '", "type":"' + type + '" }';
    var title = "Đang tải hóa đơn...";
    var url = "/Tracuu2/PrintInvoice";
    var actionFile = "DOWNLOAD";
    PrintInvoice(url, model, title, actionFile);
}

function buyerSignature() {
    var SignalrConnection;
    $.connection.hub.url = "http://localhost:19898/signalr";
    SignalrConnection = $.connection.invoiceHub;

    if (SignalrConnection == null) {
        bootbox.alert("Chưa bật plugin ký. Vui lòng kiểm tra (hoặc nhấn nút Tải Plugin). Tải lại trang web để thực hiện chức năng");
        return false;
    }
    $.connection.hub.start().done(function () {
        var mst = $("#mst_html").val();
        var invInvoiceAuthId = $("#invoiceauth").val();
        var a = $("#abcdefg").val();
        bootbox.dialog({
            title: "Đang ký hóa đơn",
            message: "<p class='text-center' ><i style='font-size:350%;' class='fa fa-spin fa-spinner'></i></p>",
            buttons: {
                cancel: {
                    label: '<i class="fa fa-times"></i> Hủy',
                    className: 'btn-danger',
                    callback: function () {
                        clearTimeout(interVal);
                        bootbox.hideAll();
                    }
                }
            }
        });

        var interVal = setTimeout(function () {
            SignalrConnection.server.SignatureXML(mst, invInvoiceAuthId, atob(a)).done(function (result) {
                console.log(result);
                if (result === "") {
                    bootbox.alert({
                        message: "Ký hóa đơn thành công. Vui lòng nhấn xem lại hóa đơn",
                        callback: function () {
                            $("#myModal").removeClass("in");
                            $(".modal-backdrop").remove();
                            $("#myModal").hide();
                            bootbox.hideAll();
                        }
                    });
                }
                else {
                    var mst = result.replace("Mã số thuế người mua không đúng ! ", "");
                    if (result.includes("Mã số thuế người mua không đúng")) {
                        result = "Mã số thuế của chứng thư số không khớp với mã số thuế bên mua trên hóa đơn " + mst;
                    }
                    bootbox.hideAll();
                    clearTimeout(interVal);
                    bootbox.alert({
                        size: "small",
                        title: "Error",
                        message: result
                    });
                }
            });
        }, 3000);
    }).fail(function () {
        bootbox.alert("Kết nối Plugin ký thất bại. Vui lòng kiểm tra lại");
    });
};

function readSignature(id) {
    var SignalrConnection;
    $.connection.hub.url = "http://localhost:19898/signalr";
    SignalrConnection = $.connection.invoiceHub;
    if (SignalrConnection == null) {
        bootbox.alert("Chưa bật plugin ký. Vui lòng kiểm tra (hoặc nhấn nút Tải Plugin). Tải lại trang web để thực hiện chức năng");
        return false;
    }
    var xml = $("#ecd").val();
    if (id === "buyer") {
        if (xml.indexOf("buyer") === -1) {
            bootbox.alert("Không có thông tin người mua");
            return false;
        }
    }
    $.connection.hub.start().done(function () {
        var arg = {
            xml: xml,
            id: id
        };

        SignalrConnection.server.execCommand("ShowCert2", JSON.stringify(arg)).done(function (result) {
            console.log(result);
        }).fail(function (error) {
            console.log('Invocation of NewContosoChatMessage failed. Error: ' + error);
        });
    }).fail(function () {
        bootbox.alert("Kết nối Plugin ký thất bại. Vui lòng kiểm tra lại");
    });
};


function veryfyXml() {
    var data = new FormData();
    var file = $('input[type=file]')[0].files[0];
    data.append('file', file);
    var xhr = new XMLHttpRequest();
    xhr.open('POST', '/TracuuFile/VeryfyXml', true);
    xhr.responseType = 'json';
    xhr.send(data);
    xhr.onload = function () {
        var a = xhr.response;
        if (a.hasOwnProperty("error")) {
            bootbox.alert({
                message: a.error,
                className: "alertCss"
            });
        } else {
            bootbox.alert({
                message: a.ok,
                className: "alertCss"
            });
        }
    }
};

function b64toBlob(b64Data, contentType, sliceSize) {
    contentType = contentType || '';
    sliceSize = sliceSize || 512;
    var byteCharacters = atob(b64Data);
    var byteArrays = [];
    for (var offset = 0; offset < byteCharacters.length; offset += sliceSize) {
        var slice = byteCharacters.slice(offset, offset + sliceSize);
        var byteNumbers = new Array(slice.length);
        for (var i = 0; i < slice.length; i++) {
            byteNumbers[i] = slice.charCodeAt(i);
        }
        var byteArray = new Uint8Array(byteNumbers);
        byteArrays.push(byteArray);
    }
    var blob = new Blob(byteArrays, { type: contentType });
    return blob;
};

function CreateTable() {
    var masoThue = $("#MaSoThue").val();
    var mauso = $("#MauSo").val();
    var kyhieu = $("#KyHieu").val();
    var data = {
        mst: masoThue,
        mau_so: mauso,
        ky_hieu: kyhieu
    };

    bootbox.dialog({
        title: "Đang kiểm tra thông tin thông báo phát hành",
        message: "<p class='text-center' ><i style='font-size:350%;' class='fa fa-spin fa-spinner'></i></p>",
        buttons: {
            cancel: {
                label: '<i class="fa fa-times"></i> Hủy',
                className: 'btn-danger',
                callback: function () {
                    clearTimeout(interVal);
                    bootbox.hideAll();
                }
            }
        }
    });


    var interVal = setTimeout(function () {
        $.ajax({
            url: '/Tracuu2/TraCuTBPH',
            method: 'POST',
            contentType: 'application/json',
            dataType: 'json',
            data: JSON.stringify(data),
            success: function (response) {
                bootbox.hideAll();
                if (response.hasOwnProperty("ok")) {
                    $("#exampleModal").modal();
                    var rs = response.ok;
                    var html = '<div class="fix-1">';
                    html += '<table class="table table-hover">';
                    html +=
                        '<h1 style="text-align: center !important;"> Hoá đơn đã được Thông báo phát hành chi tiết như sau:</h1>';
                    html += '<tr>';
                    var flag = 0;
                    $.each(rs[0],
                        function (index, value) {
                            var headerName = index;
                            switch (index) {
                                case 'date':
                                    headerName = "Ngày thông báo";
                                    break;
                                case 'no':
                                    headerName = "Số thông báo";
                                    break;
                                case 'dataUsing':
                                    headerName = "Ngày bắt đầu SD";
                                    break;
                                case 'formNo':
                                    headerName = "Mẫu số";
                                    break;
                                case 'symbol':
                                    headerName = "Ký hiệu";
                                    break;
                                case 'quantity':
                                    headerName = "Số lượng";
                                    break;
                                case 'from':
                                    headerName = "Từ số";
                                    break;
                                case 'to':
                                    headerName = "Đến số";
                                    break;
                                case 'providerName':
                                    headerName = "Tại đơn vị";
                                    break;
                                case 'providerTax':
                                    headerName = "Mst đơn vị";
                                    break;
                                case 'dateConvert':
                                    headerName = "Số đặt in";
                                    break;
                                case 'dataUsingConvert':
                                    headerName = "Ngày đặt in";
                                    break;
                                default:
                            }
                            html += '<th class ="fix-th">' + headerName + '</th>';
                        });
                    html += '</tr>';
                    $.each(rs,
                        function (index, value) {
                            html += '<tr>';
                            $.each(value,
                                function (index2, value2) {
                                    html += '<td class = "fix-td">' + value2 + '</td>';
                                });
                            html += '<tr>';
                        });
                    html += '</table> </div>';
                    console.log(html);
                    $('.content-notification').html('');
                    $('.content-notification').html(html);
                } else {
                    bootbox.alert(response.error);
                }
                $('.fix-th').css({ 'text-align': 'center !important' });
                $('.table-hover').css({ 'textbackground-color': 'antiquewhite !important' });

            },
            error: function (jqXhr, textStatus, errorThrown) {
                bootbox.hideAll();
                bootbox.alert("Không tìm thấy thông tin thông báo phát hành");
            }
        });
    }, 1000);
};

function handleFileSelect(evt) {
    var files = evt.target.files;
    var arr = ['application/zip', 'application/octet-stream', 'application/x-zip-compressed', 'multipart/x-zip'];
    if (arr.indexOf(files[0].type) === -1) {
        bootbox.alert("Định dạng File không đúng (*.zip) !");
        return;
    } else {
        if (files[0].size <= 0) {
            bootbox.alert("Bạn chưa chọn File !");
            return;
        } else {
            $(':button[type="submit"]').prop('disabled', false);
        }
    }
};

function displayInvoice(sobaomat, mst, auth, abc) {
    var data = {
        sobaomat: sobaomat,
        masothue: mst,
        type: "PDF"
    };
    $("#title-load").show();
    $("#hs-masthead").hide();
    $("#htm-content").empty();
    $("#btn-buyer-sign").hide();
    $("#btn-print-pdf").hide();
    $("#btn-download-html").hide();
    $("#btn-dowd-zip").hide();
    $("#btn-download-pdf").hide();
    $("#btn-download").hide();
    $("#btn-plugin").hide();
    $("#btn-download-pdf-inchuyendoi").hide();

    $.ajax({
        url: "/Tracuu2/PrintInvoicePdf",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify(data),
        success: function (response) {
            if (response.hasOwnProperty("ok")) {
                var builder = '';
                var blob = b64toBlob(response.ok, 'application/pdf');
                var blobUrl = URL.createObjectURL(blob);
                builder += '<iframe class="responsive-iframe" src="' + blobUrl + '" frameborder="0" height="700px" width="100%"></iframe>';
                $("#htm-content").html('');
                $("#htm-content").append(builder);
                $("#htm-content").show();
                $("#title-load").hide();
                $("#mst_html").val(mst);
                $("#sobaomat_html").val(sobaomat);
                $("#invoiceauth").val(auth);
                $("#ecd").val(response.ecd);
                $("#fileName").val(response.fileName);
                $("#abcdefg").val(abc);
                $("#btn-buyer-sign").show();
                $("#btn-download-html").show();
                $("#btn-download").show();
                $("#btn-dowd-zip").show();
                $("#btn-plugin").show();
                $("#btn-download-pdf").show();
                $("#btn-print-pdf").show();
                $("#btn-download-pdf-inchuyendoi").show();
                $("#sohoadon_view").html($("#sohoadon").val());
                $("#tientrcthue_view").html($("#tientrcthue").val());
                $("#tienthue_view").html($("#tienthue").val());
                $("#tongtien_view").html($("#tongtien").val());

                $("#MaSoThue_view").html($("#MaSoThue").val());
                $("#KyHieu_view").html($("#KyHieu").val());
                $("#MauSo_view").html($("#MauSo").val());
            } else {
                $("#htm-content").html(response.error);
                $("#btn-buyer-sign").hide();
                $("#btn-download").hide();
                $("#btn-dowdload-html").hide();
                $("#btn-dowd-zip").hide();
                $("#btn-plugin").hide();
                $("#btn-download-pdf").hide();
                $("#btn-print-pdf").hide();
                $("#btn-download-pdf-inchuyendoi").hide();
            }
        }
    });
};

function setData(result) {
    $("#sohoadon_view").html(result.data["data"][0].inv_invoiceNumber);
    $("#tientrcthue_view").html(result.data["data"][0].inv_TotalAmountWithoutVat.toLocaleString());
    $("#tienthue_view").html(result.data["data"][0].inv_vatAmount.toLocaleString());
    $("#tongtien_view").html(result.data["data"][0].inv_TotalAmount.toLocaleString());
    $("#MaSoThue").val(result.data["data"][0].mst);
    $("#KyHieu").val(result.data["data"][0].inv_invoiceSeries.trim());
    $("#MauSo").val(result.data["data"][0].mau_hd);
    $("#MaSoThueNguoiMua").val(result.data["data"][0].inv_buyerTaxCode);

    $("#invoiceauth").val(result.data["data"][0].inv_InvoiceAuth_id.trim());
    $("#ecd").val(result.ecd);
    $("#fileName").val(result.fileName);
    $("#abcdefg").val(result.data["data"][0].inv_auth_id.trim());
}

function displayInvoiceVer2(e) {
    e.preventDefault();
    bootbox.dialog({
        title: "Đang tra cứu hóa đơn",
        message: "<p class='text-center' ><i style='font-size:350%;' class='fa fa-spin fa-spinner'></i></p>",
        buttons: {
            cancel: {
                label: '<i class="fa fa-times"></i> Hủy',
                className: 'btn-danger',
                callback: function () {
                    clearTimeout(interVal);
                    bootbox.hideAll();
                }
            }
        }
    });


    var interVal = setTimeout(function () {
        var $form = $('#frmIndex');
        var model = getFormData($form);
        model.type = "PDF";
        var dataObject = JSON.stringify(model);
        $.ajax({
            url: "/Tracuu2/PrintInvoicePdf",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            data: dataObject,
            success: function (result) {
                if (!result.hasOwnProperty("error")) {
                    bootbox.hideAll();
                    $("#myModal").modal();
                    var builder = '';
                    var blob = b64toBlob(result.ok, 'application/pdf');
                    var blobUrl = URL.createObjectURL(blob);
                    builder += '<iframe class="responsive-iframe" src="' + blobUrl + '" frameborder="0" height="700px" width="100%"></iframe>';
                    $("#htm-content").html('');
                    $("#htm-content").append(builder);
                    $("#htm-content").show();
                    $("#title-load").hide();
                    $("#mst_html").val(model.masothue);
                    $("#sobaomat_html").val(model.sobaomat);
                    setData(result);
                    $("#fix-table").show();
                }
                else {
                    bootbox.hideAll();
                    clearTimeout(interVal);
                    bootbox.alert(result.error);
                }
            }
        });
    }, 1000);
};

function loadData() {
    bootbox.dialog({
        title: "Đang tra cứu hóa đơn",
        message: "<p class='text-center' ><i style='font-size:350%;' class='fa fa-spin fa-spinner'></i></p>",
        buttons: {
            cancel: {
                label: '<i class="fa fa-times"></i> Hủy',
                className: 'btn-danger',
                callback: function () {
                    clearTimeout(interVal);
                    bootbox.hideAll();
                }
            }
        }
    });
    var interVal = setTimeout(function () {
        var $form = $('#frmIndex');
        var model = getFormData($form);
        var dataObject = JSON.stringify(model);
        $.ajax({
            url: "/Tracuu2/GetInfoInvoice",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            data: dataObject,
            success: function (result) {
                if (!result.hasOwnProperty("error")) {
                    $("#divInvoice").show();
                    $('#divInvoice').prop('hidden', false);
                    $('#tableInvoice').prop('hidden', false);
                    $("#tableInvoice tbody>tr").remove();
                    for (var i = 0; i < result.data.length; i++) {
                        $("#sohoadon").val(result.data[i].inv_invoiceNumber);
                        $("#tientrcthue").val(result.data[i].inv_TotalAmountWithoutVat.toLocaleString());
                        $("#tienthue").val(result.data[i].inv_vatAmount.toLocaleString());
                        $("#tongtien").val(result.data[i].inv_TotalAmount.toLocaleString());
                        $("#MaSoThue").val(result.data[i].mst);
                        $("#KyHieu").val(result.data[i].inv_invoiceSeries);
                        $("#MauSo").val(result.data[i].mau_hd);

                        var tien = result.data[i].inv_TotalAmount == null ? result.data[i].sum_tien.toLocaleString() : result.data[i].inv_TotalAmount.toLocaleString();
                        $("#tableInvoice").find('tbody')
                            .append('<tr><td>' + result.data[i].mau_hd + '</td><td>' + result.data[i].inv_invoiceSeries + '</td><td>' + result.data[i].inv_invoiceNumber + '</td>' +
                                '<td>' + moment(result.data[i].inv_invoiceIssuedDate).format("DD/MM/YYYY") + '</td>' +
                                '<td>' + tien + '</td>' +
                                '<td>' +
                                '<a href="#" onClick = "displayInvoice(\'' + result.data[i].sobaomat.toString() + '\',\'' + result.data[i].mst.toString() + '\',\'' + result.data[i].inv_InvoiceAuth_id.toString() + '\',\'' + result.data[i].inv_auth_id.toString() + '\');" data-toggle="modal" data-target="#myModal" data-placement="top" title="Xem hóa đơn"><i class="fas fa-search text-success"></i></a>'
                                +
                                '</td></tr>');
                    }
                    bootbox.hideAll();
                    clearTimeout(interVal);
                }
                else {
                    $("#tableInvoice tbody>tr").remove();
                    bootbox.hideAll();
                    clearTimeout(interVal);
                    bootbox.alert(result.error);
                }
            }
        });
    }, 1000);
}


function GetInfoByMaSoThue() {
    var maSoThueNguoiMua = $("#MaSoThueNguoiMua").val();
    var model = {
        masothue: maSoThueNguoiMua
    }

    bootbox.dialog({
        title: "Đang kiểm tra thông tin",
        message: "<p class='text-center' ><i style='font-size:350%;' class='fa fa-spin fa-spinner'></i></p>",
        buttons: {
            cancel: {
                label: '<i class="fa fa-times"></i> Hủy',
                className: 'btn-danger',
                callback: function () {
                    clearTimeout(interVal);
                    bootbox.hideAll();
                }
            }
        }
    });


    var interVal = setTimeout(function () {
        var url = "https://demo.minvoice.com.vn/api/System/GetInfoByMST";
        $.ajax({
            url: url,
            method: 'POST',
            contentType: 'application/json',
            dataType: 'json',
            data: JSON.stringify(model),
            success: function (response) {
                bootbox.hideAll();
                if (response.hasOwnProperty("ten_cty")) {
                    $("#exampleModal").modal();
                    var html = '<div class="fix-1">';
                    html += '<table class="table table-hover">';
                    html +=
                        '<h1 style="text-align: center !important;"> Hoá đơn đã được Thông báo phát hành chi tiết như sau:</h1>';

                    html += '<thead><tr>';
                    html += '<th>Tên khách hàng</th>';
                    html += '<th>Địa chỉ</th>';
                    html += '<th>Mã số thuế</th>';
                    html += '</tr> </thead>';

                    html += ' <tbody><tr>';
                    html += '<th>' + response.ten_cty + '</th>';
                    html += '<th>' + response.dia_chi + '</th>';
                    html += '<th>' + response.ma_so_thue + '</th>';
                    html += '</tr></tbody>';

                    html += ' </table></div>';
                    $('.content-notification').html('');
                    $('.content-notification').html(html);
                } else {
                    bootbox.alert(response.error);
                }
            },
            error: function (jqXhr, textStatus, errorThrown) {
                bootbox.hideAll();
            }
        });
    }, 1000);
}



function getFormData($form) {
    var unindexedArray = $form.serializeArray();
    var indexedArray = {};
    $.map(unindexedArray, function (n, i) {
        indexedArray[n['name']] = n['value'];
    });
    return indexedArray;
};

$(document).ready(function () {
    $("#myModal").on("hidden.bs.modal", function () {
        $("#hs-masthead").show();
    });


    $("#btnInvoice").click(function (e) {
        displayInvoiceVer2(e);
    });

    $('#input-file-now').change(function (e) {
        $("#file-input-name").html('');
        var fileName = e.target.files[0].name;
        $("#file-input-name").html(fileName);
    });

    $('#btnUpFile').click(function () {
        bootbox.dialog({
            title: "Đang tra cứu hóa đơn",
            message: "<p class='text-center' ><i style='font-size:350%;' class='fa fa-spin fa-spinner'></i></p>",
            buttons: {
                cancel: {
                    label: '<i class="fa fa-times"></i> Hủy',
                    className: 'btn-danger',
                    callback: function () {
                        clearTimeout(interVal);
                        bootbox.hideAll();
                    }
                }
            }
        });

        var interVal = setTimeout(function () {
            var data = new FormData();
            var file = $('input[type=file]')[0].files[0];
            data.append('file', file);
            var xhr = new XMLHttpRequest();
            xhr.open('POST', '/TracuuFile/UploadInv', true);
            xhr.send(data);
            xhr.responseType = 'arraybuffer';
            xhr.onload = function (rs) {
                if (this.status === 200) {
                    var filename = "";
                    var disposition = xhr.getResponseHeader('Content-Disposition');
                    if (disposition && disposition.indexOf('attachment') !== -1) {
                        var filenameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;
                        var matches = filenameRegex.exec(disposition);
                        if (matches != null && matches[1]) filename = matches[1].replace(/['"]/g, '');
                    }
                    var type = xhr.getResponseHeader('Content-Type');
                    var blob = typeof File === 'function'
                        ? new File([this.response],
                            filename,
                            {
                                type: type
                            })
                        : new Blob([this.response],
                            {
                                type: type
                            });
                    if (typeof window.navigator.msSaveBlob !== 'undefined') {
                        window.navigator.msSaveBlob(blob, filename);
                    } else {
                        var URL = window.URL || window.webkitURL;
                        var downloadUrl = URL.createObjectURL(blob);

                        if (filename) {
                            var a = document.createElement("a");
                            if (typeof a.download === 'undefined') {
                                bootbox.hideAll();
                                var newWindow = window.open('/');
                                newWindow.onload = () => {
                                    newWindow.location = downloadUrl;
                                };
                            } else {
                                bootbox.hideAll();
                                var newWindow = window.open('/');
                                newWindow.onload = () => {
                                    newWindow.location = downloadUrl;
                                };
                            }
                        } else {
                            bootbox.hideAll();
                            var newWindow = window.open('/');
                            newWindow.onload = () => {
                                newWindow.location = downloadUrl;
                            };
                        }
                    }
                } else {
                    var a = JSON.parse(new TextDecoder("utf-8").decode(new Uint8Array(xhr.response)));
                    if (a.hasOwnProperty("error")) {
                        bootbox.alert(a.error);
                        bootbox.hideAll();
                    }
                }
            };
        }, 1000);
    });

    $('input[type=radio][name=radio]').change(function () {
        if (this.value === 'ckd1') {
            $("#divSearchMain").show();
            $("#divSearchByFile").hide();
        }
        else if (this.value === 'ckd2') {
            $("#divSearchMain").hide();
            $("#divSearchByFile").show();
            $("#divInvoice").hide();
        }
    });
});

