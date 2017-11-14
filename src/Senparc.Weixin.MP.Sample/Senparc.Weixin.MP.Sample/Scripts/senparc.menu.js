var senparc = {};
var maxSubMenuCount = 5;
var menuState;
var currect_i = 0;
var currect_j = 0;
senparc.menu = {
    token: '',
    conditionalmenu: null,
    defaultmenu: null,
    init: function () {
        menuState = $('#menuState');

        $('#buttonDetails').hide();
        $('#menuEditor').hide();

        $("#buttonDetails_type").change(senparc.menu.typeChanged);
        $("#addConditionalArea_list").change(senparc.menu.menuChanged);

        $(':input[id^=menu_button]').click(function () {
            $('#buttonDetails').show();
            var idPrefix = $(this).attr('data-root')
                ? ('menu_button' + $(this).attr('data-root'))
                : ('menu_button' + $(this).attr('data-j') + '_sub_button' + $(this).attr('data-i'));

            currect_i = parseInt($(this).attr('data-i'));
            currect_j = parseInt($(this).attr('data-j'));
            var keyId = idPrefix + "_key";
            var nameId = idPrefix + "_name";
            var typeId = idPrefix + "_type";
            var urlId = idPrefix + "_url";
            var mediaIdId = idPrefix + "_mediaid";

            var txtDetailsKey = $('#buttonDetails_key');
            var txtDetailsName = $('#buttonDetails_name');
            var ddlDetailsType = $('#buttonDetails_type');
            var txtDetailsUrl = $('#buttonDetails_url');
            var txtMediaId = $('#buttonDetails_mediaId');

            var hiddenButtonKey = $('#' + keyId);
            var hiddenButtonType = $('#' + typeId);
            var hiddenButtonUrl = $('#' + urlId);
            var hiddenButtonMediaId = $('#' + mediaIdId);

            txtDetailsKey.val(hiddenButtonKey.val());
            txtDetailsName.val($('#' + nameId).val());
            ddlDetailsType.val(hiddenButtonType.val());
            txtDetailsUrl.val(hiddenButtonUrl.val());
            txtMediaId.val(hiddenButtonMediaId.val());

            senparc.menu.typeChanged();

            txtDetailsKey.unbind('blur').blur(function () {
                hiddenButtonKey.val($(this).val());
            });
            ddlDetailsType.unbind('blur').blur(function () {
                hiddenButtonType.val($(this).val());
            });
            txtDetailsUrl.unbind('blur').blur(function () {
                hiddenButtonUrl.val($(this).val());
            });
            txtMediaId.unbind('blur').blur(function () {
                hiddenButtonMediaId.val($(this).val());
            });

            //修改当前行列样式
            var row = parseInt($(this).attr('data-i'));
            var column = parseInt($(this).attr('data-j'));
            $('#menuTable input').removeClass('currentMenuInput');
            $('#menuTable thead th').removeClass('currentMenuItem');
            $('#menuTable tbody td').removeClass('currentMenuItem');
            $(this).addClass('currentMenuInput');
            $('#menuTable thead th').eq(column + 1).addClass('currentMenuItem');
            $('#menuTable tbody tr').eq(row).find('td').eq(0).addClass('currentMenuItem');

            //一级菜单提示
            if (row == 5) {
                $('#rootButtonNotice').show();
            } else {
                $('#rootButtonNotice').hide();
            }
        });

        $('#up').click(function () {
            if (currect_i > 0) {
                exchange(currect_i, currect_j, currect_i - 1, currect_j);
            }
        });
        $('#down').click(function () {
            if (currect_i < 4) {
                exchange(currect_i, currect_j, currect_i + 1, currect_j);
            }
        });
        $('#left').click(function () {
            if (currect_j > 0) {
                exchange(currect_i, currect_j, currect_i, currect_j - 1);
            }
        });
        $('#right').click(function () {
            if (currect_j < 2) {
                exchange(currect_i, currect_j, currect_i, currect_j + 1);
            }
        });
        function exchange(oi, oj, ni, nj) {
            console.log(arguments);
            var o_idPrefix = 'menu_button' + oj + '_sub_button' + oi;
            var o_key = $('#' + o_idPrefix + "_key").val();
            var o_name = $('#' + o_idPrefix + "_name").val();
            var o_type = $('#' + o_idPrefix + "_type").val();
            var o_url = $('#' + o_idPrefix + "_url").val();
            var o_mediaid = $('#' + o_idPrefix + "_mediaid").val();

            var idPrefix = 'menu_button' + nj + '_sub_button' + ni;
            $('#' + o_idPrefix + "_key").val($('#' + idPrefix + "_key").val());
            $('#' + o_idPrefix + "_name").val($('#' + idPrefix + "_name").val());
            $('#' + o_idPrefix + "_type").val($('#' + idPrefix + "_type").val());
            $('#' + o_idPrefix + "_url").val($('#' + idPrefix + "_url").val());
            $('#' + o_idPrefix + "_mediaid").val($('#' + idPrefix + "_mediaid").val());

            $('#' + idPrefix + "_key").val(o_key);
            $('#' + idPrefix + "_name").val(o_name);
            $('#' + idPrefix + "_type").val(o_type);
            $('#' + idPrefix + "_url").val(o_url);
            $('#' + idPrefix + "_mediaid").val(o_mediaid);

            currect_i = parseInt(ni);
            currect_j = parseInt(nj);
            $('#' + idPrefix + "_name").click();
        }

        $('#menuLogin_Submit').click(function () {
            $.getJSON('/Menu/GetToken?t=' + Math.random(), { appId: $('#menuLogin_AppId').val(), appSecret: $('#menuLogin_AppSecret').val() },
                function (json) {
                    if (json.access_token) {
                        senparc.menu.setToken(json.access_token);
                    } else {
                        alert(json.error || '执行过程有错误，请检查！');
                    }
                });
        });

        $('#menuLogin_SubmitOldToken').click(function () {
            senparc.menu.setToken($('#menuLogin_OldToken').val());
        });

        $('#btnGetMenu').click(function () {
            menuState.html('获取菜单中...');
            $.getJSON('/Menu/GetMenu?t=' + Math.random(), { token: senparc.menu.token }, function (json) {
                if (json && json.menu) {
                    senparc.menu.defaultmenu = json.menu;
                    $(':input[id^=menu_button]:not([id$=_type])').val('');
                    $('#buttonDetails:input').val('');
                    $('#group_id, #sex, #country, #province, #city, #client_platform_type').val("");

                    var buttons = json.menu.button;
                    //此处i与j和页面中反转
                    for (var i = 0; i < buttons.length; i++) {
                        var button = buttons[i];
                        $('#menu_button' + i + '_name').val(button.name);
                        $('#menu_button' + i + '_key').val(button.key);
                        $('#menu_button' + i + '_type').val(button.type || 'click');
                        $('#menu_button' + i + '_url').val(button.url);
                        $('#menu_button' + i + '_appid').val(button.appid);
                        $('#menu_button' + i + '_pagepath').val(button.pagepath);
                        $('#menu_button' + i + '_mediaid').val(button.media_id);

                        if (button.sub_button && button.sub_button.length > 0) {
                            //二级菜单
                            for (var j = 0; j < button.sub_button.length; j++) {
                                var subButton = button.sub_button[j];
                                var idPrefix = '#menu_button' + i + '_sub_button' + j;
                                $(idPrefix + "_name").val(subButton.name);
                                $(idPrefix + "_type").val(subButton.type || 'click');
                                $(idPrefix + "_key").val(subButton.key);
                                $(idPrefix + "_url").val(subButton.url);
                                $(idPrefix + "_appid").val(subButton.appid);
                                $(idPrefix + "_pagepath").val(subButton.pagepath);
                                $(idPrefix + "_mediaid").val(subButton.media_id);
                            }
                        } else {
                            //底部菜单
                            //...
                        }
                    }

                    //显示JSON
                    $('#txtReveiceJSON').text(JSON.stringify(json));

                    if (json.conditionalmenu) {
                        senparc.menu.conditionalmenu = json.conditionalmenu;
                        $('#addConditionalArea_list').empty();
                        $('#addConditionalArea_list').append("<option value=\"-1\" selected=\"selected\">默认菜单</option>");
                        for (var i = 0; i < senparc.menu.conditionalmenu.length; i++) {
                            var cmenu = senparc.menu.conditionalmenu[i];
                            $('#addConditionalArea_list').append("<option value=\"" + i + "\">Menu - " + cmenu.menuid + "</option>");
                        }
                    }

                    menuState.html('菜单获取已完成');
                } else {
                    menuState.html(json.error || '执行过程有错误，请检查！');
                }
            });
        });

        $('#btnDeleteMenu').click(function () {
            if (!confirm('确定要删除菜单吗？此操作无法撤销！')) {
                return;
            }

            menuState.html('删除菜单中...');
            $.getJSON('/Menu/DeleteMenu?t=' + Math.random(), { token: senparc.menu.token }, function (json) {
                if (json.Success) {
                    menuState.html('删除成功，如果是误删，并且界面上有最新的菜单状态，可以立即点击【更新到服务器】按钮。');
                } else {
                    menuState.html(json.Message);
                }
            });
        });

        $('#submitMenu').click(function () {
            if (!confirm('确定要提交吗？此操作无法撤销！')) {
                return;
            }

            menuState.html('上传中...');

            $('#form_Menu').ajaxSubmit({
                dataType: 'json',
                success: function (json) {
                    if (json.Success) {
                        menuState.html('上传成功');
                    } else {
                        menuState.html(json.Message);
                    }
                }
            });
        });

        $('#submitJsonMenu').click(function () {
            if (!confirm('此方法只能更新自定义菜单（不包含个性化菜单），确定要提交吗？此操作无法撤销！')) {
                return;
            }

            menuState.html('上传中...');
            var jsonStr = $('#txtReveiceJSON').val();

            //console.log(jsonStr);

            $.post('/Menu/CreateMenuFromJson', { token: $('#tokenStr').val(), fullJson: jsonStr }, function (json) {
                if (json.Success) {
                    menuState.html('上传成功');
                } else {
                    menuState.html(json.Message);
                }
            });
        });

        $('#btnResetAccessToken').click(function () {
            $('#menuEditor').hide();
            $('#menuLogin').show();
        });

        $('#menuTable .control-input').hover(function () {
            var row = parseInt($(this).attr('data-i'));
            var column = parseInt($(this).attr('data-j'));

            $('#menuTable thead th').removeClass('hoverMenuItem');
            $('#menuTable tbody td').removeClass('hoverMenuItem');

            $('#menuTable thead th').eq(column + 1).addClass('hoverMenuItem');
            $('#menuTable tbody tr').eq(row).find('td').eq(0).addClass('hoverMenuItem');
        }, function () {
            $('#menuTable thead th').removeClass('hoverMenuItem');
            $('#menuTable tbody td').removeClass('hoverMenuItem');
        });
    },
    typeChanged: function () {
        var val = $('#buttonDetails_type').val().toUpperCase();
        switch (val) {
            case 'CLICK':
                $('#buttonDetails_key_area').slideDown(100);
                $('#buttonDetails_url_area').slideUp(100);
                $('#buttonDetails_miniprogram_appid_area').slideUp(100);
                $('#buttonDetails_miniprogram_pagepath_area').slideUp(100);
                $('#buttonDetails_mediaId_area').slideUp(100);
                break;
            case 'VIEW':
                $('#buttonDetails_key_area').slideUp(100);
                $('#buttonDetails_url_area').slideDown(100);
                $('#buttonDetails_miniprogram_appid_area').slideUp(100);
                $('#buttonDetails_miniprogram_pagepath_area').slideUp(100);
                $('#buttonDetails_mediaId_area').slideUp(100);
                break;
            case 'MINIPROGRAM':
                $('#buttonDetails_key_area').slideUp(100);
                $('#buttonDetails_url_area').slideDown(100);
                $('#buttonDetails_miniprogram_appid_area').slideDown(100);
                $('#buttonDetails_miniprogram_pagepath_area').slideDown(100);
                $('#buttonDetails_mediaId_area').slideUp(100);
                break;
            case 'MEDIA_ID':
            case 'VIEW_LIMITED':
                $('#buttonDetails_key_area').slideUp(100);
                $('#buttonDetails_url_area').slideUp(100);
                $('#buttonDetails_miniprogram_appid_area').slideUp(100);
                $('#buttonDetails_miniprogram_pagepath_area').slideUp(100);
                $('#buttonDetails_mediaId_area').slideDown(100);
                break;
            default:
                $('#buttonDetails_key_area').slideDown(100);
                $('#buttonDetails_url_area').slideUp(100);
                $('#buttonDetails_miniprogram_appid_area').slideUp(100);
                $('#buttonDetails_miniprogram_pagepath_area').slideUp(100);
                $('#buttonDetails_mediaId_area').slideUp(100);
                break;
        }
    },
    menuChanged: function () {
        var val = parseInt($('#addConditionalArea_list').val());
        var menu = senparc.menu.defaultmenu;
        if (val >= 0 && senparc.menu.conditionalmenu != null && val < senparc.menu.conditionalmenu.length)
            menu = senparc.menu.conditionalmenu[val];
        if (menu) {
            $(':input[id^=menu_button]:not([id$=_type])').val('');
            $('#buttonDetails:input').val('');

            $('#buttonDetails').hide();

            var buttons = menu.button;
            //此处i与j和页面中反转
            for (var i = 0; i < buttons.length; i++) {
                var button = buttons[i];
                $('#menu_button' + i + '_name').val(button.name);
                $('#menu_button' + i + '_key').val(button.key);
                $('#menu_button' + i + '_type').val(button.type || 'click');
                $('#menu_button' + i + '_url').val(button.url);
                $('#menu_button' + i + '_appid').val(button.appid);
                $('#menu_button' + i + '_pagepath').val(button.pagepath);
                $('#menu_button' + i + '_mediaid').val(button.media_id);

                if (button.sub_button && button.sub_button.length > 0) {
                    //二级菜单
                    for (var j = 0; j < button.sub_button.length; j++) {
                        var subButton = button.sub_button[j];
                        var idPrefix = '#menu_button' + i + '_sub_button' + j;
                        $(idPrefix + "_name").val(subButton.name);
                        $(idPrefix + "_type").val(subButton.type || 'click');
                        $(idPrefix + "_key").val(subButton.key);
                        $(idPrefix + "_url").val(subButton.url);
                        $(idPrefix + "_appid").val(subButton.appid);
                        $(idPrefix + "_pagepath").val(subButton.pagepath);
                        $(idPrefix + "_mediaid").val(subButton.media_id);
                    }
                } else {
                    //底部菜单
                    //...
                }
            }
            if (menu.matchrule) {
                $('#group_id').val(menu.matchrule.group_id ? menu.matchrule.group_id : "");
                $('#sex').val(menu.matchrule.sex ? menu.matchrule.sex : "");
                $('#country').val(menu.matchrule.country ? menu.matchrule.country : "");
                $('#province').val(menu.matchrule.province ? menu.matchrule.province : "");
                $('#city').val(menu.matchrule.city ? menu.matchrule.city : "");
                $('#client_platform_type').val(menu.matchrule.client_platform_type ? menu.matchrule.client_platform_type : "");
            } else {
                $('#group_id, #sex, #country, #province, #city, #client_platform_type').val("");
            }
        }
    },
    setToken: function (token) {
        senparc.menu.token = token;
        $('#tokenStr').val(token);
        $('#menuEditor').show();
        $('#menuLogin').hide();
    }
};