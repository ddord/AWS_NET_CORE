$(document).ready(function () {
    $('#menuList').find('a').each(function () {
        if ($(this).attr('href').toLowerCase() == window.location.pathname.toLowerCase()) {
            $(this).closest('div').addClass("selected");
        }
    });

    fnDocumentReadyCommon();
});

var m_OptionPageSize = 15;
var m_OptionPageCount = 10;
var m_TotalCount = 0;
var m_TotalPageCount = 0;
var m_CurrentPage = 1;
var m_Page = {
    Set: function (totalCnt, getListFunctionName) {
        if (totalCnt == 0) {
            $('#pageField').html("");
        } else {
            var pageCnt = parseInt(totalCnt / m_OptionPageSize) + (totalCnt % m_OptionPageSize > 0 ? 1 : 0);
            var pageHtml = "";

            m_TotalCount = totalCnt;
            m_TotalPageCount = pageCnt;
            m_CurrentPage = 1;

            pageHtml += '   <li><a id="PageFirst" class="page-link first" onclick="m_Page.MoveFirst(this);"><i class="icon-fux-arrow-first-bold"></i></a></li>';
            pageHtml += '   <li><a id="PagePrev" class="page-link prev" onclick="m_Page.MovePrev(this);"><i class="icon-fux-arrow-left-line-bold"></i></a></li>';
            for (var i = 1; i <= pageCnt; i++) {
                if (i > m_OptionPageCount) {
                    pageHtml += '<li><a id="page_' + i + '" onclick="' + getListFunctionName + '(' + i + ', false);" class="page-link" style="display:none;">' + i + '</a></li>';
                } else {
                    pageHtml += '<li><a id="page_' + i + '" onclick="' + getListFunctionName + '(' + i + ', false);" class="page-link">' + i + '</a></li>';
                }
            }
            pageHtml += '   <li><a id="PageNext" class="page-link next" onclick="m_Page.MoveNext(this);"><i class="icon-fux-arrow-right-line-bold"></i></a></li>';
            pageHtml += '   <li><a id="PageLast" class="page-link last" onclick="m_Page.MoveLast(this);"><i class="icon-fux-arrow-last-bold"></i></a></li>';

            $('#pageField').html(pageHtml);
            $('#page_1').addClass('active');
        }
    },
    MoveFirst: function (elem) {
        if (!$(elem).hasClass('disabled')) {
            $('#pageField').find('[id^=page_]').eq(0).trigger('click');
        }
    },
    MoveLast: function (elem) {
        if (!$(elem).hasClass('disabled')) {
            $('#pageField').find('[id^=page_]').eq($('#pageField').find('[id^=page_]').length - 1).trigger('click');
        }
    },
    MovePrev: function (elem) {
        if (!$(elem).hasClass('disabled')) {
            var moveNum = parseInt($('#pageField').find('.active').attr('id').replace('page_', '')) - 1;
            $('#page_' + moveNum).trigger('click');
        }
    },
    MoveNext: function (elem) {
        if (!$(elem).hasClass('disabled')) {
            var moveNum = parseInt($('#pageField').find('.active').attr('id').replace('page_', '')) + 1;
            $('#page_' + moveNum).trigger('click');
        }
    },
    AfterMove: function () {
        $('#pageField').find('[id^=page_]').hide();
        $('#pageField').find('.active').removeClass('active');
        $('#page_' + m_CurrentPage).addClass('active');

        var showStart = 1;
        var showEnd = 1;

        if (m_TotalPageCount <= m_OptionPageCount) {
            showStart = 1;
            showEnd = m_TotalPageCount;
        } else {
            var leftShift = Math.floor(m_OptionPageCount / 2);
            var rightShift = Math.floor(m_OptionPageCount / 2);

            if (m_CurrentPage <= leftShift) {
                showStart = 1;
                showEnd = m_OptionPageCount;
            } else if (m_CurrentPage + rightShift >= m_TotalPageCount) {
                showStart = m_TotalPageCount - m_OptionPageCount + 1;
                showEnd = m_TotalPageCount;
            } else {
                showStart = m_CurrentPage - leftShift;
                showEnd = m_CurrentPage + rightShift;
            }
        }

        for (var i = showStart; i <= showEnd; i++) {
            $('#page_' + i).show();
        }

        $('#PageFirst').removeClass('disabled');
        $('#PagePrev').removeClass('disabled');
        $('#PageNext').removeClass('disabled');
        $('#PageLast').removeClass('disabled');

        if (m_CurrentPage == 1) {
            $('#PageFirst').addClass('disabled');
            $('#PagePrev').addClass('disabled');
        } else if (m_CurrentPage == m_TotalPageCount) {
            $('#PageNext').addClass('disabled');
            $('#PageLast').addClass('disabled');
        }
    }
}

var m_Modal = {
    Open: function (url, target, width, height) {
        if (url != null) {
            $('#modalPopupContent').load(url);
        }
        $('#modalPopup').show();
    },
    Close: function () {
        $('#modalPopup').hide();
    },
    ParentFunction: function (fnName, p1, p2, p3, p4, p5) {
        if (typeof window[fnName] == "function") {
            window[fnName](p1, p2, p3, p4, p5);
        }
    },
    Move: function (url) {
        url = window.location.origin + url;
        $('#modalPopupContent').load(url);
    }
}

var m_Popup = {
    Open: function (url, target, width, height) {
        url = window.location.origin + url;

        width = width || 500;
        height = height || 500;

        var dualScreenLeft = window.screenLeft != undefined ? window.screenLeft : screen.left;
        var dualScreenTop = window.screenTop != undefined ? window.screenTop : screen.top;
        var leftwidth = window.innerWidth ? window.innerWidth : document.documentElement.clientWidth ? document.documentElement.clientWidth : screen.width;
        var rightheight = window.innerHeight ? window.innerHeight : document.documentElement.clientHeight ? document.documentElement.clientHeight : screen.height;
        var left = ((leftwidth / 2) - (width / 2)) + dualScreenLeft;
        var top = ((rightheight / 2) - (height / 2)) + dualScreenTop;

        return window.open(url, target, 'location=no, scrollbars=yes, menubar=no, status=no, titlebar=no, resizable=no, width=' + width + ', height=' + height);
    },
    Close: function () {
        window.close();
    },
    Submit: function (callbackName, p1, p2, p3, p4, p5) {
        if (typeof window.opener[callbackName] == "function") {
            window.opener[callbackName](p1, p2, p3, p4, p5);
        }

        m_Popup.Close();
    },
    Move: function (url) {
        url = window.location.origin + url;
        window.location = url;
    }
}

var m_Ajax = {
    Call: function (url, param, callback, errorCallback, isAsync) {
        param = param || {};
        isAsync = (isAsync == null ? true : isAsync);

        $.ajax({
            type: "POST",
            url: url,
            data: param,
            dataType: "json",
            async: isAsync,
            success: function (data) {
                return callback(data);
            },
            error: function (response) {
                if (errorCallback) {
                    return errorCallback(response);
                } else {
                    alert(response.responseText);
                }
            }

        });
    }
}

var m_Required = {
    Check: function () {
        var isAccept = true;

        if ($('[m_required]').length > 0) {
            var msg = "";

            $('[m_required]').each(function () {
                var keyword = $(this).attr('m_required');
                var value = "";
                var isInputHidden = false;

                if (keyword != "") {
                    switch ($(this).prop('tagName').toLowerCase()) {
                        case "textarea":
                            msg = "Please enter {0}.";
                            value = $(this).val();
                            break;
                        case "select":
                            msg = "Please select {0}.";
                            value = $(this).val();
                            break;
                        case "input":
                            switch ($(this).attr('type')) {
                                case "text":
                                case "password":
                                    msg = "Please enter {0}.";
                                    value = $(this).val();
                                    break;
                                case "hidden":
                                    msg = "Please enter {0}.";
                                    value = $(this).val();
                                    isInputHidden = true;
                                    break;
                                case "checkbox":
                                case "radio":
                                    msg = "Please select {0}.";
                                    var checkedCnt = 0;
                                    $("[m_required=" + $(this).attr('m_required') + "]").each(function () {
                                        if ($(this).is(":checked")) {
                                            checkedCnt++;
                                        }
                                    });

                                    if (checkedCnt > 0) {
                                        value = true;
                                    } else {
                                        value = false;
                                    }
                                    break;
                            }
                            break;
                    }
                }

                if (isInputHidden) {
                    if (value == false || value == "" || value == null || value == -1) {
                        if (value != '0') {
                            msg = msg.replace("{0}", keyword);
                            alert(msg);
                            isAccept = false;
                            return false;
                        }
                    }
                } else {
                    if (value == false || value == "" || value == null || value == -1) {
                        if (value != '0') {
                            msg = msg.replace("{0}", keyword);
                            alert(msg);
                            isAccept = false;
                            return false;
                        }
                    }
                }
            });
        }

        return isAccept;
    }
}

var m_Upload = {
    File: function (fileform, url, callbackName) {
        $.ajax({
            type: "POST",
            url: url,
            data: fileform,
            enctype: 'multipart/form-data',
            cache: false,
            processData: false,
            contentType: false,
            success: function (data) {
                m_Upload.Callback(callbackName, data);
            },
            error: function (request, status, error) {
                alert("code:" + request.status + "\n" + "message:" + request.responseText + "\n" + "error:" + error);
            }
        });
    },
    Callback: function (callbackName, p1, p2, p3, p4, p5) {
        if (typeof window[callbackName] == "function") {
            window[callbackName](p1, p2, p3, p4, p5);
        }
    }
}

var formData = new FormData();
var _FILE_MAX_SIZE = 500;

function fnSelectFile() {
    var maxsize = _FILE_MAX_SIZE * 1024 * 1024;
    var input = document.getElementById('fileInput');

    for (var i = 0; i < input.files.length; ++i) {
        var name = input.files.item(i).name;
        var size = input.files.item(i).size;

        if (size > maxsize) {
            alert("Files can be less than or equal to " + _FILE_MAX_SIZE + "Byte.");
        }
        else {
            formData.append("files", input.files[i]);
        }
    }

    fnDisplayFileList();
}

function fnCancelFile(name, size) {
    var filelist = formData.getAll("files");
    formData.delete("files");
    for (var file of filelist) {
        if (file.name == name && file.size == size) {
            continue;
        }
        formData.append("files", file);
    }

    fnDisplayFileList();
}

function fnDisplayFileList() {
    var children = "";
    var output = document.getElementById('fileList');
    filelist = formData.getAll("files");
    output.innerHTML = children;

    for (var file of filelist) {
        var name = file.name;
        var size = file.size;
        children += '📎&nbsp;&nbsp;&nbsp;' + name + ' ';
        children += "<span style='cursor:pointer;color:red;' onclick='javascript:fnCancelFile(\"" + name + "\", " + size + ");'>✖</span><Br />";
    }

    output.innerHTML = children;
}


