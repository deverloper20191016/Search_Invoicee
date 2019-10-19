/*
* Kendo UI Localization Project for v2012.3.1114
* Copyright 2012 Telerik AD. All rights reserved.
*
* Standard German (de-DE) Language Pack
*
* Project home : https://github.com/loudenvier/kendo-global
* Kendo UI home : http://kendoui.com
* Author : SINNOVA
*
*
* This project is released to the public domain, although one must abide to the
* licensing terms set forth by Telerik to use Kendo UI, as shown bellow.
*
* Telerik's original licensing terms:
* -----------------------------------
* Kendo UI Web commercial licenses may be obtained at
* https://www.kendoui.com/purchase/license-agreement/kendo-ui-web-commercial.aspx
* If you do not own a commercial license, this file shall be governed by the
* GNU General Public License (GPL) version 3.
* For GPL requirements, please review: http://www.gnu.org/copyleft/gpl.html
*/

kendo.ui.Locale = "Viet Nam (vi-VN)";
kendo.ui.ColumnMenu.prototype.options.messages =
  $.extend(kendo.ui.ColumnMenu.prototype.options.messages, {

      /* COLUMN MENU MESSAGES
      ****************************************************************************/
      sortAscending: "Tăng dần",
      sortDescending: "Giảm dần",
      filter: "Lọc",
      columns: "Cột",
      done: "Hoàn thanh",
      settings: "Cài đặt cột",
      lock: "Khóa",
      unlock: "Mở khóa"
      /***************************************************************************/
  });

kendo.ui.Groupable.prototype.options.messages =
  $.extend(kendo.ui.Groupable.prototype.options.messages, {

      /* GRID GROUP PANEL MESSAGES
      ****************************************************************************/
      empty: "Drag a column header and drop it here to group by that column"
      /***************************************************************************/
  });

kendo.ui.FilterMenu.prototype.options.messages =
  $.extend(kendo.ui.FilterMenu.prototype.options.messages, {

      /* FILTER MENU MESSAGES
      ***************************************************************************/
      info: "Điều kiện:", // sets the text on top of the filter menu
      isTrue: "đúng", // sets the text for "isTrue" radio button
      isFalse: "sai", // sets the text for "isFalse" radio button
      filter: "Lọc", // sets the text for the "Filter" button
      clear: "Xóa", // sets the text for the "Clear" button
      and: "Và",
      or: "Hoặc",
      selectValue: "-Lựa chọn-",
      operator: "Toán tử",
      value: "Giá trị",
      cancel: "Hủy"
      /***************************************************************************/
  });

kendo.ui.FilterMenu.prototype.options.operators =
  $.extend(kendo.ui.FilterMenu.prototype.options.operators, {

      /* FILTER MENU OPERATORS (for each supported data type)
      ****************************************************************************/
      string: {
          eq: "Bằng",
          neq: "Không bằng",
          startswith: "Bắt đầu bằng",
          contains: "Chứa",
          doesnotcontain: "Không chứa",
          endswith: "Kết thúc bằng",
          isnull: "Rỗng",
          isnotnull: "Không rỗng",
          isempty: "Trống",
          isnotempty: "Không trống"
      },
      number: {
          eq: "Bằng",
          neq: "Không bằng",
          gte: "Lớn hơn hoặc bằng",
          gt: "Lớn hơn",
          lte: "Nhỏ hơn hoặc bằng",
          lt: "Nhỏ hơn",
          isnull: "Rỗng",
          isnotnull: "Không rỗng"
      },
      date: {
          eq: "Bằng",
          neq: "Không bằng",
          gte: "Lớn hơn hoặc bằng",
          gt: "Lớn hơn",
          lte: "Nhỏ hơn hoặc bằng",
          lt: "Nhỏ hơn",
          isnull: "Rỗng",
          isnotnull: "Không rỗng"
      },
      enums: {
          eq: "Bằng",
          neq: "Không bằng",
          isnull: "Rỗng",
          isnotnull: "Không rỗng"
      }
      /***************************************************************************/
  });

kendo.ui.Validator.prototype.options.messages =
  $.extend(kendo.ui.Validator.prototype.options.messages, {

      /* VALIDATOR MESSAGES
      ****************************************************************************/
      required: "{0} is required",
      pattern: "{0} is not valid",
      min: "{0} should be greater than or equal to {1}",
      max: "{0} should be smaller than or equal to {1}",
      step: "{0} is not valid",
      email: "{0} is not valid email",
      url: "{0} is not valid URL",
      date: "Không đúng định dạng ngày/tháng/năm"
      /***************************************************************************/
  });

kendo.ui.ImageBrowser.prototype.options.messages =
  $.extend(kendo.ui.ImageBrowser.prototype.options.messages, {

      /* IMAGE BROWSER MESSAGES
      ****************************************************************************/
      uploadFile: "Upload",
      orderBy: "Arrange by",
      orderByName: "Name",
      orderBySize: "Size",
      directoryNotFound: "A directory with this name was not found.",
      emptyFolder: "Empty Folder",
      deleteFile: 'Are you sure you want to delete "{0}"?',
      invalidFileType: "The selected file \"{0}\" is not valid. Supported file types are {1}.",
      overwriteFile: "A file with name \"{0}\" already exists in the current directory. Do you want to overwrite it?",
      dropFilesHere: "drop files here to upload"
      /***************************************************************************/
  });

kendo.ui.Editor.prototype.options.messages =
  $.extend(kendo.ui.Editor.prototype.options.messages, {

      /* EDITOR MESSAGES
      ****************************************************************************/
      bold: "Bold",
      italic: "Italic",
      underline: "Underline",
      strikethrough: "Strikethrough",
      superscript: "Superscript",
      subscript: "Subscript",
      justifyCenter: "Center text",
      justifyLeft: "Align text left",
      justifyRight: "Align text right",
      justifyFull: "Justify",
      insertUnorderedList: "Insert unordered list",
      insertOrderedList: "Insert ordered list",
      indent: "Indent",
      outdent: "Outdent",
      createLink: "Insert hyperlink",
      unlink: "Remove hyperlink",
      insertImage: "Insert image",
      insertHtml: "Insert HTML",
      fontName: "Select font family",
      fontNameInherit: "(inherited font)",
      fontSize: "Select font size",
      fontSizeInherit: "(inherited size)",
      formatBlock: "Format",
      foreColor: "Color",
      backColor: "Background color",
      style: "Styles",
      emptyFolder: "Empty Folder",
      uploadFile: "Upload",
      orderBy: "Arrange by:",
      orderBySize: "Size",
      orderByName: "Name",
      invalidFileType: "The selected file \"{0}\" is not valid. Supported file types are {1}.",
      deleteFile: 'Are you sure you want to delete "{0}"?',
      overwriteFile: 'A file with name "{0}" already exists in the current directory. Do you want to overwrite it?',
      directoryNotFound: "A directory with this name was not found.",
      imageWebAddress: "Web address",
      imageAltText: "Alternate text",
      dialogInsert: "Insert",
      dialogButtonSeparator: "or",
      dialogCancel: "Cancel"
      /***************************************************************************/
  });

/* NumericTextBox messages */

kendo.ui.NumericTextBox.prototype.options =
    $.extend(true, kendo.ui.NumericTextBox.prototype.options, {
        upArrowText: "Tăng giá trị",
        downArrowText: "Giảm giá trị"
    });


/* ColorPicker messages */

kendo.ui.ColorPicker.prototype.options.messages =
   $.extend(true, kendo.ui.ColorPicker.prototype.options.messages, {
       apply: "Áp dụng",
       cancel: "Hủy"
   });

/* FlatColorPicker messages */

kendo.ui.FlatColorPicker.prototype.options.messages =
   $.extend(true, kendo.ui.FlatColorPicker.prototype.options.messages, {
       apply: "Áp dụng",
       cancel: "Hủy"
   });

/* Pager messages */

kendo.ui.Pager.prototype.options.messages =
   $.extend(true, kendo.ui.Pager.prototype.options.messages, {
       allPages: "Tất cả",
       display: "{0} - {1} trong {2} bản ghi",
       empty: "Không có bản ghi nào để hiển thị",
       page: "Trang",
       of: "/ {0}",
       itemsPerPage: "Số bản ghi / trang",
       first: "Trang đầu",
       previous: "Trước",
       next: "Sau",
       last: "Trang cuối",
       refresh: "Làm mới",
       morePages: "Nhiều trang hơn"
   });
