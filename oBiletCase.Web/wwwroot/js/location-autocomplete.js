// Akıllı lokasyon araması: debounce'lu combobox, eşleşme vurgusu, yükleme/boş
// durumları ve klavye gezintisi. jQuery/Bootstrap kullanmaz.
(function () {
  "use strict";

  const DEBOUNCE_MS = 250;
  const MIN_QUERY_LENGTH = 0;
  const REQUEST_TIMEOUT_MS = 15000;

  function debounce(fn, delay) {
    let timerId;
    return function debounced(...args) {
      window.clearTimeout(timerId);
      timerId = window.setTimeout(() => fn(...args), delay);
    };
  }

  function escapeHtml(text) {
    return text
      .replaceAll("&", "&amp;")
      .replaceAll("<", "&lt;")
      .replaceAll(">", "&gt;")
      .replaceAll('"', "&quot;");
  }

  function highlightMatch(name, query) {
    const safeName = escapeHtml(name);
    const trimmed = query.trim();
    if (!trimmed) {
      return safeName;
    }

    const safeQuery = escapeHtml(trimmed).replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
    return safeName.replace(new RegExp(`(${safeQuery})`, "ig"), "<mark>$1</mark>");
  }

  function initLocationAutocomplete(root, options) {
    const input = root.querySelector("[data-autocomplete-input]");
    const list = root.querySelector("[data-autocomplete-list]");
    const idField = root.querySelector("[data-autocomplete-id]");
    const nameField = root.querySelector("[data-autocomplete-name]");
    const clearButton = root.querySelector("[data-autocomplete-clear]");

    let currentOptions = [];
    let activeIndex = -1;
    let requestToken = 0;

    function closeList() {
      list.hidden = true;
      input.setAttribute("aria-expanded", "false");
      input.removeAttribute("aria-activedescendant");
      activeIndex = -1;
    }

    function openList() {
      list.hidden = false;
      input.setAttribute("aria-expanded", "true");
    }

    function syncClearButton() {
      if (!clearButton) {
        return;
      }

      clearButton.hidden = input.value.length === 0;
    }

    function selectOption(option) {
      idField.value = option.id;
      nameField.value = option.name;
      input.value = option.name;
      syncClearButton();
      closeList();
      input.dispatchEvent(new CustomEvent("locationselected", { bubbles: true, detail: option }));
    }

    function clearSelection() {
      idField.value = "";
      nameField.value = "";
      syncClearButton();
    }

    function renderStatus(text, isLoading) {
      currentOptions = [];
      activeIndex = -1;
      list.innerHTML = "";

      const status = document.createElement("li");
      status.className = isLoading ? "autocomplete__status" : "autocomplete__empty";
      status.setAttribute("role", "presentation");

      if (isLoading) {
        const spinner = document.createElement("span");
        spinner.className = "spinner";
        spinner.setAttribute("aria-hidden", "true");
        status.appendChild(spinner);
      }

      const label = document.createElement("span");
      label.textContent = text;
      status.appendChild(label);
      list.appendChild(status);
      openList();
    }

    function renderOptions(items, query) {
      currentOptions = items;
      activeIndex = -1;
      list.innerHTML = "";

      if (items.length === 0) {
        renderStatus(options.emptyText, false);
        return;
      }

      items.forEach((item, index) => {
        const option = document.createElement("li");
        option.className = "autocomplete__option";
        option.id = `${root.id}-option-${index}`;
        option.role = "option";
        option.setAttribute("aria-label", item.name);

        const mark = document.createElement("span");
        mark.className = "autocomplete__option-mark";
        mark.setAttribute("aria-hidden", "true");
        mark.textContent = (item.name || "?").charAt(0).toUpperCase();

        const label = document.createElement("span");
        label.innerHTML = highlightMatch(item.name, query);

        option.appendChild(mark);
        option.appendChild(label);
        option.addEventListener("mousedown", (event) => {
          event.preventDefault();
          selectOption(item);
        });
        list.appendChild(option);
      });

      openList();
    }

    function highlight(index) {
      const optionEls = list.querySelectorAll(".autocomplete__option");
      optionEls.forEach((el) => el.removeAttribute("aria-selected"));

      if (index >= 0 && index < optionEls.length) {
        optionEls[index].setAttribute("aria-selected", "true");
        optionEls[index].scrollIntoView({ block: "nearest" });
        input.setAttribute("aria-activedescendant", optionEls[index].id);
      } else {
        input.removeAttribute("aria-activedescendant");
      }
    }

    const fetchOptions = debounce(async (query) => {
      const token = ++requestToken;
      renderStatus(options.loadingText, true);

      const abortController = new AbortController();
      const timeoutId = window.setTimeout(() => abortController.abort(), REQUEST_TIMEOUT_MS);

      try {
        const response = await fetch(`${options.searchUrl}?query=${encodeURIComponent(query)}`, {
          headers: { Accept: "application/json" },
          signal: abortController.signal,
        });

        if (token !== requestToken) {
          return;
        }

        if (!response.ok) {
          renderStatus(options.errorText, false);
          return;
        }

        const items = await response.json();
        renderOptions(items, query);
      } catch {
        if (token !== requestToken) {
          return;
        }

        renderStatus(options.errorText, false);
      } finally {
        window.clearTimeout(timeoutId);
      }
    }, DEBOUNCE_MS);

    input.addEventListener("input", () => {
      clearSelection();

      const query = input.value.trim();
      if (query.length < MIN_QUERY_LENGTH) {
        closeList();
        return;
      }

      fetchOptions(query);
    });

    input.addEventListener("focus", () => {
      if (input.value.trim().length >= MIN_QUERY_LENGTH) {
        fetchOptions(input.value.trim());
      }
    });

    input.addEventListener("keydown", (event) => {
      if (list.hidden && (event.key === "ArrowDown" || event.key === "ArrowUp")) {
        fetchOptions(input.value.trim());
        return;
      }

      if (event.key === "ArrowDown") {
        event.preventDefault();
        activeIndex = Math.min(activeIndex + 1, currentOptions.length - 1);
        highlight(activeIndex);
      } else if (event.key === "ArrowUp") {
        event.preventDefault();
        activeIndex = Math.max(activeIndex - 1, 0);
        highlight(activeIndex);
      } else if (event.key === "Enter") {
        if (activeIndex >= 0 && currentOptions[activeIndex]) {
          event.preventDefault();
          selectOption(currentOptions[activeIndex]);
        }
      } else if (event.key === "Escape") {
        closeList();
      }
    });

    if (clearButton) {
      clearButton.addEventListener("click", () => {
        input.value = "";
        clearSelection();
        input.focus();
        fetchOptions("");
      });
    }

    document.addEventListener("click", (event) => {
      if (!root.contains(event.target)) {
        closeList();
      }
    });

    syncClearButton();

    return {
      setValue(id, name) {
        idField.value = id ?? "";
        nameField.value = name ?? "";
        input.value = name ?? "";
        syncClearButton();
      },
      getValue() {
        return { id: idField.value, name: nameField.value };
      },
    };
  }

  window.oBiletCase = window.oBiletCase || {};
  window.oBiletCase.initLocationAutocomplete = initLocationAutocomplete;
})();
