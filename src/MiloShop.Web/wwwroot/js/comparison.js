// Loaded after the first compare interaction; product data is Razor-encoded.
const selected = new Map();
const bar = document.getElementById("compare-bar");
const count = document.getElementById("compare-count");
const open = document.getElementById("compare-open");
const dialog = document.getElementById("compare-dialog");
function refresh(message) {
  bar.hidden = selected.size === 0;
  count.textContent =
    message || `${selected.size} محصول انتخاب شده؛ حداقل ۲ و حداکثر ۳ محصول.`;
  open.disabled = selected.size < 2;
}
export function update(input) {
  if (
    input.checked &&
    !selected.has(input.dataset.compare) &&
    selected.size === 3
  ) {
    input.checked = false;
    refresh("حداکثر سه محصول را می‌توانید مقایسه کنید.");
    return;
  }
  if (input.checked) selected.set(input.dataset.compare, { ...input.dataset });
  else selected.delete(input.dataset.compare);
  document.querySelectorAll("[data-compare]").forEach((other) => {
    if (other.dataset.compare === input.dataset.compare)
      other.checked = input.checked;
  });
  refresh();
}
document.getElementById("compare-clear").addEventListener("click", () => {
  selected.clear();
  document
    .querySelectorAll("[data-compare]")
    .forEach((input) => (input.checked = false));
  refresh();
});
document
  .getElementById("compare-close")
  .addEventListener("click", () => dialog.close());
open.addEventListener("click", () => {
  const table = document.createElement("table");
  for (const [key, label] of [
    ["title", "محصول"],
    ["price", "قیمت (تومان)"],
    ["brand", "برند"],
    ["material", "جنس / ویژگی"],
    ["sku", "کد محصول"],
  ]) {
    const row = table.insertRow();
    const heading = document.createElement("th");
    heading.scope = "row";
    heading.textContent = label;
    row.appendChild(heading);
    for (const product of selected.values())
      row.insertCell().textContent = product[key] || "ثبت نشده";
  }
  document.getElementById("compare-table").replaceChildren(table);
  dialog.showModal();
});
