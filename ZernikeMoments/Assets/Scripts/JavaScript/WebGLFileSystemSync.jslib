mergeInto(LibraryManager.library, {
  SyncFilesystem: function () {
    // Llama a la función de sincronización de Emscripten
    FS.syncfs(false, function (err) {
      if (err) {
        console.log('Error al sincronizar WebGL FS:', err);
      } else {
        console.log('WebGL FS sincronizado exitosamente.');
      }
    });
  }
});