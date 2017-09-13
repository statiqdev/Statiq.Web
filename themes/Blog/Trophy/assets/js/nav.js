var Nav = (function() {
    var s;
  
    return {
      settings: {
          sideNav: document.getElementsByClassName('sidenav')[0],
        open: document.getElementsByClassName('openbtn')[0],
        close: document.getElementsByClassName('closebtn')[0],
        openWidth: "250px"
      },
  
      init: function() {
        s = this.settings;
        this.open();
        this.close();
      },

      open: function() {
 
            s.open.addEventListener('click', function() {
                s.sideNav.style.width=s.openWidth;
            }); 
      },
      
      close:function(){
        s.close.addEventListener('click', function() {
            s.sideNav.style.width=0;
        }); 
      }
    }
  })();

  document.addEventListener('DOMContentLoaded', function() {
    Nav.init();
  });