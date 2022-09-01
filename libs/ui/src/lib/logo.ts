export class LogoElement extends HTMLElement {
  public static observedAttributes = ['url'];

  get url() {
    return this.getAttribute('url');
  }

  set url(val) {
    if(val) {
      this.setAttribute('url', val);
    } else {
      this.removeAttribute('url');
    }
  }

  attributeChangedCallback() {
    console.log('this.url', this.url);
    this.innerHTML = `<img width="64" height="64" src="${this.url}"/>`;
  }
}
customElements.define('eworks-logo', LogoElement);
