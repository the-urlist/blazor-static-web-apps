window.THEURLIST = {
  sortable: {
    init: (id) => {
      new Sortable(document.getElementById(id), { 
        handle: ".drag-handle", 
        animation: 200,
        forceFallback: true
      });
    }
  },
  focusElement: (id) => {
    setTimeout(() => {
      const element = document.getElementById(id); 
      element.focus();
    }, 0);
  }
}
