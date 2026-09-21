// Index arama formunun davranışı: lokasyon değiştirme (kayma animasyonu),
// özel tarih seçici, son 5 aramanın localStorage ile hatırlanması. jQuery/Bootstrap yok.
(function () {
  "use strict";

  const STORAGE_KEY = "oBiletCase.recentSearches";
  const MAX_RECENT_SEARCHES = 5;
  const SWAP_MS = 420;

  function isSameSearch(a, b) {
    return a.originId === b.originId
      && a.destinationId === b.destinationId
      && a.departureDate === b.departureDate;
  }

  function saveRecentSearches(searches) {
    try {
      window.localStorage.setItem(STORAGE_KEY, JSON.stringify(searches.slice(0, MAX_RECENT_SEARCHES)));
    } catch {
      // localStorage kullanılamıyorsa (gizli mod vb.) geçmiş arama hatırlatma sessizce atlanır.
    }
  }

  function saveRecentSearch(search) {
    const withoutDuplicate = loadRecentSearches().filter((item) => !isSameSearch(item, search));
    saveRecentSearches([search, ...withoutDuplicate]);
  }

  function removeRecentSearch(search) {
    saveRecentSearches(loadRecentSearches().filter((item) => !isSameSearch(item, search)));
  }

  function loadRecentSearches() {
    try {
      const raw = window.localStorage.getItem(STORAGE_KEY);
      const parsed = raw ? JSON.parse(raw) : [];
      return Array.isArray(parsed)
        ? parsed.filter((item) => item && item.originId && item.destinationId && item.departureDate)
        : [];
    } catch {
      return [];
    }
  }

  function formatDisplayDate(isoDate) {
    if (!isoDate) {
      return "";
    }

    const [year, month, day] = isoDate.split("-").map(Number);
    const localDate = new Date(year, month - 1, day);

    return new Intl.DateTimeFormat(document.documentElement.lang || undefined, {
      day: "numeric",
      month: "long",
      year: "numeric",
    }).format(localDate);
  }

  function prefersReducedMotion() {
    return window.matchMedia("(prefers-reduced-motion: reduce)").matches;
  }

  function wait(ms) {
    return new Promise((resolve) => window.setTimeout(resolve, ms));
  }

  // İkon / alan boşluğuna tıklanınca ilgili input veya tarih trigger'ına focus verir.
  function wireFieldFocus(form) {
    form.querySelectorAll(".search-bar__field").forEach((field) => {
      field.addEventListener("mousedown", (event) => {
        if (event.target.closest("[data-autocomplete-clear], [data-autocomplete-list], [data-datepicker-popover], .datepicker__day, .datepicker__nav-btn")) {
          return;
        }

        const input = field.querySelector("[data-autocomplete-input]");
        const dateTrigger = field.querySelector("[data-datepicker-trigger]");
        const focusTarget = input || dateTrigger;

        if (!focusTarget || focusTarget === event.target || focusTarget.contains(event.target)) {
          return;
        }

        event.preventDefault();
        focusTarget.focus();
      });
    });
  }

  async function animateLocationSwap(originControl, destinationControl, swapButton, applySwap) {
    if (prefersReducedMotion()) {
      applySwap();
      return;
    }

    const originRect = originControl.getBoundingClientRect();
    const destinationRect = destinationControl.getBoundingClientRect();
    const deltaX = destinationRect.left - originRect.left;
    const deltaY = destinationRect.top - originRect.top;

    originControl.classList.add("is-swapping");
    destinationControl.classList.add("is-swapping");
    swapButton.classList.add("is-swapping");

    // Force style application before transforming.
    void originControl.offsetWidth;

    originControl.style.transform = `translate(${deltaX}px, ${deltaY}px)`;
    destinationControl.style.transform = `translate(${-deltaX}px, ${-deltaY}px)`;

    await wait(SWAP_MS);

    applySwap();

    originControl.style.transition = "none";
    destinationControl.style.transition = "none";
    originControl.style.transform = "";
    destinationControl.style.transform = "";
    originControl.classList.remove("is-swapping");
    destinationControl.classList.remove("is-swapping");
    swapButton.classList.remove("is-swapping");

    void originControl.offsetWidth;

    originControl.style.transition = "";
    destinationControl.style.transition = "";
  }

  function init() {
    const form = document.querySelector("[data-search-form]");
    if (!form) {
      return;
    }

    const originRoot = form.querySelector("[data-autocomplete-root='origin']");
    const destinationRoot = form.querySelector("[data-autocomplete-root='destination']");
    const dateRoot = form.querySelector("[data-datepicker]");
    const swapButton = form.querySelector("[data-swap]");
    const searchUrl = form.dataset.locationsSearchUrl;
    const originField = originRoot.closest(".search-bar__field");
    const destinationField = destinationRoot.closest(".search-bar__field");
    const originControl = originField.querySelector(".search-bar__control");
    const destinationControl = destinationField.querySelector(".search-bar__control");

    const origin = window.oBiletCase.initLocationAutocomplete(originRoot, {
      searchUrl,
      emptyText: originRoot.dataset.emptyText,
      loadingText: originRoot.dataset.loadingText,
      errorText: originRoot.dataset.errorText,
    });
    const destination = window.oBiletCase.initLocationAutocomplete(destinationRoot, {
      searchUrl,
      emptyText: destinationRoot.dataset.emptyText,
      loadingText: destinationRoot.dataset.loadingText,
      errorText: destinationRoot.dataset.errorText,
    });

    form.querySelectorAll("[data-autocomplete-input]").forEach((input) => {
      input.setAttribute("spellcheck", "false");
      input.setAttribute("autocorrect", "off");
      input.setAttribute("autocapitalize", "off");
    });
    wireFieldFocus(form);
    const datePicker = window.oBiletCase.initDatePicker(dateRoot, {
      minDate: dateRoot.dataset.minDate,
      dateLabel: dateRoot.dataset.dateLabel,
    });

    function applySearch(search) {
      if (search.originId && search.originName) {
        origin.setValue(search.originId, search.originName);
      }
      if (search.destinationId && search.destinationName) {
        destination.setValue(search.destinationId, search.destinationName);
      }
      if (search.departureDate && search.departureDate >= dateRoot.dataset.minDate) {
        datePicker.setValue(search.departureDate);
      }
    }

    function renderRecentSearches(searches) {
      const section = document.querySelector("[data-recent-searches]");
      if (!section) {
        return;
      }

      const list = section.querySelector("[data-recent-searches-list]");
      const removeLabel = section.dataset.removeLabel || "";
      list.innerHTML = "";

      if (searches.length === 0) {
        section.hidden = true;
        return;
      }

      searches.forEach((search) => {
        const item = document.createElement("li");
        item.className = "recent-searches__item";

        const button = document.createElement("button");
        button.type = "button";
        button.className = "recent-searches__button";

        const route = document.createElement("span");
        route.className = "recent-searches__route";

        const originSpan = document.createElement("span");
        originSpan.className = "recent-searches__place";
        originSpan.textContent = search.originName;

        const arrow = document.createElementNS("http://www.w3.org/2000/svg", "svg");
        arrow.setAttribute("class", "recent-searches__arrow");
        arrow.setAttribute("width", "16");
        arrow.setAttribute("height", "16");
        arrow.setAttribute("viewBox", "0 0 24 24");
        arrow.setAttribute("fill", "none");
        arrow.setAttribute("aria-hidden", "true");
        const arrowPath = document.createElementNS("http://www.w3.org/2000/svg", "path");
        arrowPath.setAttribute("d", "M5 12h14M13 6l6 6-6 6");
        arrowPath.setAttribute("stroke", "currentColor");
        arrowPath.setAttribute("stroke-width", "1.5");
        arrowPath.setAttribute("stroke-linecap", "round");
        arrowPath.setAttribute("stroke-linejoin", "round");
        arrow.appendChild(arrowPath);

        const destinationSpan = document.createElement("span");
        destinationSpan.className = "recent-searches__place";
        destinationSpan.textContent = search.destinationName;

        route.append(originSpan, arrow, destinationSpan);

        const dateSpan = document.createElement("span");
        dateSpan.className = "recent-searches__date";
        dateSpan.textContent = formatDisplayDate(search.departureDate);

        button.append(route, dateSpan);
        button.addEventListener("click", () => {
          applySearch(search);
          form.requestSubmit();
        });

        const removeButton = document.createElement("button");
        removeButton.type = "button";
        removeButton.className = "recent-searches__remove";
        removeButton.setAttribute("aria-label", removeLabel);
        removeButton.textContent = "×";
        removeButton.addEventListener("click", (event) => {
          event.preventDefault();
          event.stopPropagation();
          removeRecentSearch(search);
          renderRecentSearches(loadRecentSearches());
        });

        item.append(button, removeButton);
        list.appendChild(item);
      });

      section.hidden = false;
    }

    let isSwapping = false;

    swapButton.addEventListener("click", async () => {
      if (isSwapping) {
        return;
      }

      isSwapping = true;
      swapButton.disabled = true;

      const originValue = origin.getValue();
      const destinationValue = destination.getValue();

      try {
        await animateLocationSwap(originControl, destinationControl, swapButton, () => {
          origin.setValue(destinationValue.id, destinationValue.name);
          destination.setValue(originValue.id, originValue.name);
        });
      } finally {
        isSwapping = false;
        swapButton.disabled = false;
      }
    });

    const recentSearches = loadRecentSearches();

    if (form.dataset.fresh === "true") {
      if (recentSearches[0]) {
        applySearch(recentSearches[0]);
      }

      renderRecentSearches(recentSearches);
    }

    form.addEventListener("submit", () => {
      const originValue = origin.getValue();
      const destinationValue = destination.getValue();

      saveRecentSearch({
        originId: originValue.id,
        originName: originValue.name,
        destinationId: destinationValue.id,
        destinationName: destinationValue.name,
        departureDate: datePicker.getValue(),
      });
    });
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", init);
  } else {
    init();
  }
})();
