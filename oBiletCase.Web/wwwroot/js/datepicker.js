// Custom calendar date picker. Native <input type="date"> kullanmaz; 7 sütunlu
// hizalı ızgara, klavye ve bugün/yarın kısayolları sağlar.
(function () {
  "use strict";

  function pad(value) {
    return String(value).padStart(2, "0");
  }

  function toIsoDate(date) {
    return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
  }

  function parseIso(iso) {
    const [year, month, day] = iso.split("-").map(Number);
    return new Date(year, month - 1, day);
  }

  function startOfDay(date) {
    return new Date(date.getFullYear(), date.getMonth(), date.getDate());
  }

  function getFirstWeekday(locale) {
    try {
      const info = new Intl.Locale(locale).weekInfo;
      if (info && typeof info.firstDay === "number") {
        return info.firstDay === 7 ? 0 : info.firstDay;
      }
    } catch {
      // Intl.Locale.weekInfo desteklenmiyorsa TR Pazartesi, diğerleri Pazar.
    }

    return locale.toLowerCase().startsWith("tr") ? 1 : 0;
  }

  function formatParts(date, locale) {
    const weekday = new Intl.DateTimeFormat(locale, { weekday: "short" }).format(date);
    const day = new Intl.DateTimeFormat(locale, { day: "numeric" }).format(date);
    const month = new Intl.DateTimeFormat(locale, { month: "short" }).format(date);
    const year = new Intl.DateTimeFormat(locale, { year: "numeric" }).format(date);
    return { weekday, day, month, year };
  }

  function initDatePicker(root, options) {
    const trigger = root.querySelector("[data-datepicker-trigger]");
    const popover = root.querySelector("[data-datepicker-popover]");
    const valueField = root.querySelector("[data-datepicker-value]");
    const weekdayEl = root.querySelector("[data-datepicker-weekday]");
    const valueEl = root.querySelector("[data-datepicker-display]");
    const monthLabel = root.querySelector("[data-datepicker-month-label]");
    const grid = root.querySelector("[data-datepicker-grid]");
    const weekdaysEl = root.querySelector("[data-datepicker-weekdays]");
    const prevBtn = root.querySelector("[data-datepicker-prev]");
    const nextBtn = root.querySelector("[data-datepicker-next]");
    const locale = document.documentElement.lang || "tr-TR";
    const minDate = startOfDay(options.minDate ? parseIso(options.minDate) : new Date());
    const firstWeekday = getFirstWeekday(locale);

    let selected = valueField.value ? parseIso(valueField.value) : new Date(minDate.getFullYear(), minDate.getMonth(), minDate.getDate() + 1);
    let view = new Date(selected.getFullYear(), selected.getMonth(), 1);

    function isSameDay(a, b) {
      return a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate();
    }

    function updateTrigger() {
      const parts = formatParts(selected, locale);
      weekdayEl.textContent = parts.weekday;
      valueEl.textContent = `${parts.day} ${parts.month} ${parts.year}`;
      valueField.value = toIsoDate(selected);
      trigger.setAttribute("aria-label", `${options.dateLabel}: ${valueEl.textContent}`);
    }

    function renderWeekdays() {
      weekdaysEl.innerHTML = "";
      for (let i = 0; i < 7; i += 1) {
        const date = new Date(2024, 0, 7 + firstWeekday + i);
        const cell = document.createElement("div");
        cell.className = "datepicker__weekday-cell";
        cell.textContent = new Intl.DateTimeFormat(locale, { weekday: "short" }).format(date);
        weekdaysEl.appendChild(cell);
      }
    }

    function renderGrid() {
      grid.innerHTML = "";
      monthLabel.textContent = new Intl.DateTimeFormat(locale, { month: "long", year: "numeric" }).format(view);

      const viewMonthStart = new Date(view.getFullYear(), view.getMonth(), 1);
      prevBtn.disabled = viewMonthStart <= new Date(minDate.getFullYear(), minDate.getMonth(), 1);

      const startOffset = (viewMonthStart.getDay() - firstWeekday + 7) % 7;
      const gridStart = new Date(view.getFullYear(), view.getMonth(), 1 - startOffset);
      const today = startOfDay(new Date());

      for (let i = 0; i < 42; i += 1) {
        const date = new Date(gridStart.getFullYear(), gridStart.getMonth(), gridStart.getDate() + i);
        const iso = toIsoDate(date);
        const button = document.createElement("button");
        button.type = "button";
        button.className = "datepicker__day";
        button.textContent = String(date.getDate());
        button.dataset.date = iso;
        button.setAttribute("role", "gridcell");

        if (date.getMonth() !== view.getMonth()) {
          button.classList.add("is-outside");
        }

        if (isSameDay(date, today)) {
          button.classList.add("is-today");
        }

        if (isSameDay(date, selected)) {
          button.classList.add("is-selected");
          button.setAttribute("aria-selected", "true");
        } else {
          button.setAttribute("aria-selected", "false");
        }

        if (startOfDay(date) < minDate) {
          button.disabled = true;
        }

        button.addEventListener("click", () => {
          selected = date;
          view = new Date(date.getFullYear(), date.getMonth(), 1);
          updateTrigger();
          syncQuickButtons();
          close();
        });

        grid.appendChild(button);
      }
    }

    function open() {
      popover.hidden = false;
      trigger.setAttribute("aria-expanded", "true");
      view = new Date(selected.getFullYear(), selected.getMonth(), 1);
      renderGrid();
    }

    function close() {
      popover.hidden = true;
      trigger.setAttribute("aria-expanded", "false");
    }

    function setValue(iso) {
      selected = parseIso(iso);
      view = new Date(selected.getFullYear(), selected.getMonth(), 1);
      updateTrigger();
      syncQuickButtons();
      if (!popover.hidden) {
        renderGrid();
      }
    }

    function syncQuickButtons() {
      const todayIso = toIsoDate(minDate);
      const tomorrow = new Date(minDate);
      tomorrow.setDate(tomorrow.getDate() + 1);
      const selectedIso = toIsoDate(selected);

      document.querySelectorAll("[data-quick-date]").forEach((button) => {
        const targetIso = button.dataset.quickDate === "today" ? todayIso : toIsoDate(tomorrow);
        button.classList.toggle("is-active", targetIso === selectedIso);
      });
    }

    trigger.addEventListener("click", () => {
      if (popover.hidden) {
        open();
      } else {
        close();
      }
    });

    prevBtn.addEventListener("click", () => {
      view = new Date(view.getFullYear(), view.getMonth() - 1, 1);
      renderGrid();
    });

    nextBtn.addEventListener("click", () => {
      view = new Date(view.getFullYear(), view.getMonth() + 1, 1);
      renderGrid();
    });

    document.querySelectorAll("[data-quick-date]").forEach((button) => {
      button.addEventListener("click", () => {
        const offsetDays = button.dataset.quickDate === "today" ? 0 : 1;
        const target = new Date(minDate);
        target.setDate(minDate.getDate() + offsetDays);
        setValue(toIsoDate(target));
        close();
      });
    });

    document.addEventListener("click", (event) => {
      if (!root.contains(event.target)) {
        close();
      }
    });

    document.addEventListener("keydown", (event) => {
      if (event.key === "Escape" && !popover.hidden) {
        close();
        trigger.focus();
      }
    });

    renderWeekdays();
    updateTrigger();
    syncQuickButtons();

    return {
      setValue,
      getValue() {
        return valueField.value;
      },
    };
  }

  window.oBiletCase = window.oBiletCase || {};
  window.oBiletCase.initDatePicker = initDatePicker;
})();
