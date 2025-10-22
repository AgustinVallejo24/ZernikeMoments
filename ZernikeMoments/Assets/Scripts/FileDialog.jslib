var FileDialog = {
    OpenFileDialog: function() {
    var fileInput = document.createElement("input");
    fileInput.type = "file";
    fileInput.accept = "image/*";

    fileInput.onchange = function(event) {
        var file = event.target.files[0];
        if (!file) return;

        var reader = new FileReader();
        reader.onload = function(e) {
            var dataURL = e.target.result;

            // Cambia 'ImageLoader' por el nombre de tu GameObject en Unity
            // y 'CargarImagenDesdeWeb' por el método que recibirá la imagen
            gameInstance.SendMessage('BrowseImageButton', 'CargarImagenDesdeWeb', dataURL);
        };
        reader.readAsDataURL(file);
    };

    fileInput.click();
}
};

mergeInto(LibraryManager.library, FileDialog); 