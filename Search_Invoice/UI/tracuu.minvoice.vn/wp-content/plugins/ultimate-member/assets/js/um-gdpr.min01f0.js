!function(t){"use strict";t(document).on("click","a.um-toggle-gdpr",function(){var e=jQuery(this);t(".um-gdpr-content").toggle("fast",function(){t(".um-gdpr-content").is(":visible")&&e.text(e.data("toggle-hide")),t(".um-gdpr-content").is(":hidden")&&e.text(e.data("toggle-show"))})})}(jQuery);