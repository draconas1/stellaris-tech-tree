ImageFunctions = {
  addImagesToTextNodes: function (ctx, allImagePromises) {
    // cannot do anything that is async here since this happens after draw but before scaling.
    // so async results in images being drawn afgter the rest of the view has been scaled.
    let downloadedImagesById = {};
    allImagePromises.forEach(result => downloadedImagesById[result.nodeId] = result.img);

    Object.entries(allNodes).forEach(([nodeId, node]) => {
      // do not try to add images if the node itself is an image or the node is part of a cluster
      if (node.shape !== 'image' && node.group !== 'Building' && network.findNode(nodeId).length < 2) {
        const nodeImage = downloadedImagesById[nodeId];
        if (node.hasImage && nodeImage != null) {
          const boundingbox = network.getBoundingBox([nodeId]);
          ctx.drawImage(nodeImage, boundingbox.left + 5, boundingbox.top - ((nodeImage.height - (boundingbox.bottom - boundingbox.top)) / 2));
        }

        // draw categories
        if (node.categories !== undefined) {
          for (let i = 0; i < node.categories.length; i++) {
            const category = node.categories[i];
            const categoryImage = downloadedImagesById[category];
            const boundingbox = network.getBoundingBox([nodeId]);

            // special case for single category - centralise the category
            if (node.categories.length === 1) {
              ctx.drawImage(categoryImage, boundingbox.right - 34, boundingbox.top - ((categoryImage.height - (boundingbox.bottom - boundingbox.top)) / 2));
            }
            // else draw them stacked, they will slightly overlap the box and start going outside if there is more than 2.
            else {
              const x = (boundingbox.right - 34) + (Math.floor(i / 2) * 32);
              const y = boundingbox.top + ((i % 2) * 32);
              ctx.drawImage(categoryImage, x, y);
            }
          }
        }
      }
    });
  },

  loadAllImages: function () {
    const promises = [];
    const seenCategories = {};
    Object.entries(allNodes).forEach(([nodeId, node]) => {
      if (node.hasImage) {
        const promise = new Promise((resolve, reject) => {
          const img = new Image();
          img.src = node.image;
          img.onload = resolve.bind(this, {img, nodeId});
          img.onerror = img.onabort = e => {
            console.log("Failed load tech image " + node.image + " " + e);
            resolve.bind(this, {img: null, nodeId})
          }
        });
        promises.push(promise)
      }

      if (node.categories !== undefined) {
        node.categories.forEach(category => {
          // keep a track of categories we have processed before so we don't make a lot of duplicate requests
          if (!seenCategories.hasOwnProperty(category)) {
            seenCategories[category] = category;
            const promise = new Promise((resolve, reject) => {
              const img = new Image();
              const src = "images/technologies/categories/" + category + ".png";
              img.src = src;
              img.onload = resolve.bind(this, {img, nodeId: category});
              img.onerror = img.onabort = e => {
                console.log("Failed category image " + src + " " + e);
                resolve.bind(this, {img: null, nodeId: category})
              }
            });
            promises.push(promise)
          }
        });
      }
    });

    return Promise.all(promises);
  }
};