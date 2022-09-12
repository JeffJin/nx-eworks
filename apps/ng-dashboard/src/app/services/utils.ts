export class Utils {

  static getParameterByName(name: string, url: string = null): string {
    if (!url) {
      url = window.location.href;
    }
    name = name.replace(/[\[\]]/g, '\\$&');
    const regex = new RegExp('[?&]' + name + '(=([^&#]*)|&|#|$)');
    const results = regex.exec(url);
    if (!results) {
      return null;
    }
    if (!results[2]) {
      return '';
    }
    return decodeURIComponent(results[2].replace(/\+/g, ' '));
  }

  static getExtension(filename): string {
    const parts = filename.split('.');
    return parts[parts.length - 1];
  }

  static isImage(filename): boolean {
    const ext = this.getExtension(filename);
    switch (ext.toLowerCase()) {
      case 'jpg':
      case 'gif':
      case 'bmp':
      case 'png':
        return true;
    }
    return false;
  }

  static isAudio(filename): boolean {
    const ext = this.getExtension(filename);
    switch (ext.toLowerCase()) {
      case 'mp3':
      case 'wav':
      case 'wma':
        return true;
    }
    return false;
  }

  static isVideo(filename): boolean {
    const ext = this.getExtension(filename);
    switch (ext.toLowerCase()) {
      case 'm4v':
      case 'avi':
      case 'mpg':
      case 'mp4':
      case 'mov':
        // etc
        return true;
    }
  }

  static isMediaFile(filename): boolean {
    return this.isVideo(filename) || this.isImage(filename) || this.isAudio(filename);
  }

  static sanitizeString(str): string {
    str = str.replace(/[^a-z0-9áéíóúñü \.,_-]/gim, '');
    return str.trim();
  }
}
