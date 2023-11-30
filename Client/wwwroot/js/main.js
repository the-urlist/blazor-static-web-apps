window.THEURLIST = {
  sortable: {
    init: (id, component) => {
      new Sortable(document.getElementById(id), { 
        handle: ".drag-handle", 
        animation: 200,
          forceFallback: true,
          onUpdate: (event) => {
              component.invokeMethodAsync('Drop', event.oldDraggableIndex, event.newDraggableIndex);
          }
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
