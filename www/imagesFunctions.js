ImageFunctions = {
    addImagesToTextNodes : function (ctx, allImagePromises)
    {
        // cannot do anything that is async here since this happens after draw but before scaling.
        // so async results in images being drawn afgter the rest of the view has been scaled.
        let downloadedImagesById = {};
        allImagePromises.forEach(result => downloadedImagesById[result.nodeId] = result.img);

        Object.entries(allNodes).forEach(([nodeId, node]) => {
            let img = downloadedImagesById[nodeId];
            if (node.hasImage && img != null) {
                let boundingbox = network.getBoundingBox([nodeId]);
                ctx.drawImage(img, boundingbox.right, boundingbox.top - ((img.height - (boundingbox.bottom - boundingbox.top)) / 2));
            }
        });
    }
};