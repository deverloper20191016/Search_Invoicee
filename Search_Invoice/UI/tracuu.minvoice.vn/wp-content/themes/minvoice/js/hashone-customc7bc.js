/**
 * Hashone Custom JS
 *
 * @package HashOne
 *
 * Distributed under the MIT license - http://opensource.org/licenses/MIT
 */

jQuery(function($){

  if($('#hs-bx-slider .hs-slide').length > 0){
  	$('#hs-bx-slider').bxSlider({
  		'pager': false,
  		'auto' : true,
  		'mode' : 'fade',
  		'pause' : 3000,
      'onSliderLoad': function(){
        $('.slide-banner-overlay').remove();
      }
  	});
  }


	$('.hs-testimonial-slider').bxSlider({
		'controls' : false,
		'pager': true,
		'auto' : true,
		'pause' : 5000,
		'mode' : 'fade'
	});

	$(window).scroll(function () {

      if ($(this).scrollTop() > 100) {
          $('#hs-masthead')
          .addClass('animated fadeInDown')
          .fadeIn();
          
      } else {
          $('#hs-masthead')
          .removeClass('animated fadeInDown');
      }

  });

  $(".hs_client_logo_slider").owlCarousel({
    autoPlay: 4000,
    items : 4,
    itemsDesktop : [1199,4],
    itemsDesktopSmall : [979,4],
    pagination : false
  });

  var first_class = $('.hs-portfolio-cat-name:first').data('filter');
  $('.hs-portfolio-cat-name:first').addClass('active');



	$('.hs-portfolio-cat-name-list').on( 'click', '.hs-portfolio-cat-name', function() {
	  var filterValue = $(this).attr('data-filter');
	  $container.isotope({ filter: filterValue });
	  $('.hs-portfolio-cat-name').removeClass('active');
	  $(this).addClass('active');
	});



 

  $(window).scroll(function(){
  	if($(window).scrollTop() > 300){
  		$('#hs-back-top').removeClass('bounceInRight bounceOutRight hs-hide').addClass('bounceInRight');
  	}else{
  		$('#hs-back-top').removeClass('bounceInRight bounceOutRight').addClass('bounceOutRight');
  	}
  });

  $('#hs-back-top').click(function(){
  	$('html,body').animate({scrollTop:0},800);
  });

  $('.hs-toggle-menu').click(function(){
  	$('.hs-main-navigation .hs-menu').slideToggle();
  });

  $('.hs-menu').onePageNav({
    currentClass: 'current',
    changeHash: false,
    scrollSpeed: 750,
    scrollThreshold: 0.1,
    scrollOffset: 82
  });

 // *only* if we have anchor on the url
  if(window.location.hash) {
      $('html, body').animate({
          scrollTop: $(window.location.hash).offset().top - 82
      }, 1000 );        
  }
  
 
  
  $(window).on("resize", function (e) {
        checkScreenSize();
    });

    checkScreenSize();

    function checkScreenSize(){
        var newWindowWidth = $(window).width();
        if (newWindowWidth < 1180) {
             $('li.menu-item-has-children >a').click(function(){
			  return false;
			  
		  });
        }
        
    }
  

});