window.onload = function () {
    let videoImage = document.getElementById('video_image');
    videoImage.src = `ws://${document.domain}:8080/?action=stream`;
};