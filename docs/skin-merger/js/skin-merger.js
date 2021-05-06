var canvas = document.getElementById("canvas");
var ctx = canvas.getContext("2d");

var sizeX = 0;
var sizeY = 0;

var sizeX2 = 0;
var sizeY2 = 0;
var count = 0;

function getUsername(name, url) {
    if (!url) url = window.location.href
    name = name.replace(/[\[\]]/g, '\\$&')
    var regex = new RegExp('[?&]' + name + '(=([^&#]*)|&|#|$)'),
        results = regex.exec(url)
    if (!results) return null
    if (!results[2]) return ''
    return decodeURIComponent(results[2].replace(/\+/g, ' '))
}
// ?u=UnknownShadow200&u2=Venk
var username = getUsername('u')
var username2 = getUsername('u2')

var img1 = loadImage('http://classicube.s3.amazonaws.com/skin/' + username + '.png', main);
var img2 = loadImage('http://classicube.s3.amazonaws.com/skin/' + username2 + '.png', main);

function main() {
    ctx.drawImage(img1, 0, 0, sizeX, sizeY, 0, 0, sizeX, sizeY);

    ctx.drawImage(img2, 0, 0, sizeX2, 16, 0, 0, sizeX2, 16);
}

function loadImage(src, onload) {
    count++;
    var img = new Image();

    img.onload = onload;
    img.src = src;

    if (count == 1) {
        sizeX = img.width
        sizeY = img.height
    } else if (count > 1) {
        sizeX2 = img.width
        sizeY2 = img.height
    }

    return img;
}