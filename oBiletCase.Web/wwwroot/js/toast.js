// Basit toast bildirimi — harici kütüphane yok; tasarım token'larıyla stil alır.
(function () {
  "use strict";

  const DISMISS_MS = 4800;
  const EXIT_MS = 220;

  let region = null;
  let hideTimer = null;
  let removeTimer = null;

  function prefersReducedMotion() {
    return window.matchMedia("(prefers-reduced-motion: reduce)").matches;
  }

  function ensureRegion() {
    if (region) {
      return region;
    }

    region = document.createElement("div");
    region.className = "toast-region";
    region.setAttribute("aria-live", "assertive");
    region.setAttribute("aria-atomic", "true");
    document.body.appendChild(region);
    return region;
  }

  function clearTimers() {
    window.clearTimeout(hideTimer);
    window.clearTimeout(removeTimer);
    hideTimer = null;
    removeTimer = null;
  }

  function dismissToast(toast) {
    if (!toast || toast.classList.contains("is-leaving")) {
      return;
    }

    clearTimers();
    toast.classList.remove("is-visible");
    toast.classList.add("is-leaving");

    const delay = prefersReducedMotion() ? 0 : EXIT_MS;
    removeTimer = window.setTimeout(() => {
      toast.remove();
    }, delay);
  }

  function showToast(message, options = {}) {
    const text = typeof message === "string" ? message.trim() : "";
    if (!text) {
      return;
    }

    const host = ensureRegion();
    clearTimers();
    host.replaceChildren();

    const toast = document.createElement("div");
    toast.className = "toast toast--error";
    toast.setAttribute("role", "alert");

    const icon = document.createElementNS("http://www.w3.org/2000/svg", "svg");
    icon.setAttribute("class", "toast__icon");
    icon.setAttribute("viewBox", "0 0 24 24");
    icon.setAttribute("fill", "none");
    icon.setAttribute("aria-hidden", "true");
    const iconPath = document.createElementNS("http://www.w3.org/2000/svg", "path");
    iconPath.setAttribute(
      "d",
      "M12 8.25v5M12 16.75h.01M12 3.5a8.5 8.5 0 1 0 0 17 8.5 8.5 0 0 0 0-17Z");
    iconPath.setAttribute("stroke", "currentColor");
    iconPath.setAttribute("stroke-width", "1.5");
    iconPath.setAttribute("stroke-linecap", "round");
    iconPath.setAttribute("stroke-linejoin", "round");
    icon.appendChild(iconPath);

    const body = document.createElement("p");
    body.className = "toast__message";
    body.textContent = text;

    const dismiss = document.createElement("button");
    dismiss.type = "button";
    dismiss.className = "toast__dismiss";
    dismiss.setAttribute("aria-label", options.dismissLabel || "Close");
    dismiss.textContent = "×";
    dismiss.addEventListener("click", () => dismissToast(toast));

    toast.append(icon, body, dismiss);
    host.appendChild(toast);

    // Force layout so the enter transition runs.
    void toast.offsetWidth;
    toast.classList.add("is-visible");

    hideTimer = window.setTimeout(() => dismissToast(toast), DISMISS_MS);
  }

  window.oBiletCase = window.oBiletCase || {};
  window.oBiletCase.showToast = showToast;
})();
