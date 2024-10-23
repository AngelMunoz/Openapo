import { marked } from "https://cdn.jsdelivr.net/npm/marked/lib/marked.esm.js";

const template = document.createElement("template");
template.innerHTML = `
    <section class="uk-flex">
      <div class="uk-card uk-card-default uk-card-body uk-width-1-2 uk-margin-right">
        <textarea class="markdown-input uk-textarea" rows=5 title="Blog Editor"></textarea>
      </div>
      <div class="uk-card uk-card-default uk-card-body uk-card-hover uk-width-1-2"
           style="overflow-wrap: break-word">
        <div class="markdown-preview"></div>
      </div>
    </section>
`;

class MarkdownInput extends HTMLElement {
  static observedAttributes = ["text"];
  static formAssociated = true;

  constructor() {
    super();
    this.internals = this.attachInternals();
    this.appendChild(template.content.cloneNode(true));
    this.textarea = this.querySelector("textarea");
    this.preview = this.querySelector(".markdown-preview");
    this.abortCtrl = new AbortController();
  }

  connectedCallback() {
    if (!this.abortCtrl) {
      this.abortCtrl = new AbortController();
    }
    this.textarea.addEventListener("input", () => this.onInput(), {
      signal: this.abortCtrl.signal,
    });
  }

  disconnectedCallback() {
    this.abortCtrl.abort();
    this.abortCtrl = null;
  }

  onInput() {
    this.preview.innerHTML = marked.parse(this.textarea.value);
    this.internals.setFormValue(this.textarea.value, this.textarea.value);
  }

  attributeChangedCallback(name, oldValue, newValue) {
    if (name === "text") {
      this.textarea.value = newValue;
      this.preview.innerHTML = marked.parse(newValue);
      this.internals.setFormValue(newValue, newValue);
    }
  }

  get text() {
    return this.textarea.value;
  }

  set text(value) {
    this.textarea.value = value;
    this.preview.innerHTML = marked.parse(value);
    this.internals.setFormValue(value, value);
  }
}

customElements.define("ope-markdown-input", MarkdownInput);
