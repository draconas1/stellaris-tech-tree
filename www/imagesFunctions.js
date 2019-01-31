ImageFunctions = {
    addImagesToTextNodes: function (ctx, allImagePromises) {
        // cannot do anything that is async here since this happens after draw but before scaling.
        // so async results in images being drawn afgter the rest of the view has been scaled.
        let downloadedImagesById = {};
        allImagePromises.forEach(result => downloadedImagesById[result.nodeId] = result.img);

        Object.entries(allNodes).forEach(([nodeId, node]) => {
            const img = downloadedImagesById[nodeId];
            if (node.hasImage && img != null) {
                const boundingbox = network.getBoundingBox([nodeId]);
                ctx.drawImage(img, boundingbox.left + 1 - img.width, boundingbox.top - ((img.height - (boundingbox.bottom - boundingbox.top)) / 2));
            }
        });
    },
    
    loadAllImages: function () {
        const promises = [];
        Object.entries(allNodes).forEach(([nodeId, node]) => {
            if (node.hasImage) {
                const promise = new Promise((resolve, reject) => {
                    const img = new Image();
                    img.src = node.image;
                    img.onload = resolve.bind(this, {img, nodeId});
                    img.onerror = img.onabort = e =>  {
                        console.log("Failed load load image " + node.image + " " + e);
                        resolve.bind(this, {img: null, nodeId})
                    }
                });
                promises.push(promise)
            }
        });

        return Promise.all(promises);
    }
};