window.THEURLIST = {
  sortable: {
    init: (dotnetObjRef, callback) => {
      new Sortable(document.getElementById('linkBundle'), { 
        handle: ".drag-handle",  // Drag handle selector within list items);
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
