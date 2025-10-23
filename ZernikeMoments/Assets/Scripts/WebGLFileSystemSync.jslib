mergeInto(LibraryManager.library, {
  SyncFilesystem: function () {
    // Llama a la funci�n de sincronizaci�n de Emscripten
    FS.syncfs(false, function (err) {
      if (err) {
        console.log('Error al sincronizar WebGL FS:', err);
      } else {
        console.log('WebGL FS sincronizado exitosamente.');
      }
    });
  }
});