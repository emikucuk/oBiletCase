(function () {
  "use strict";

  function bucketForHour(hour) {
    if (hour < 6) {
      return "0-6";
    }
    if (hour < 12) {
      return "6-12";
    }
    if (hour < 18) {
      return "12-18";
    }
    return "18-24";
  }

  function initJourneyFilters(root) {
    const cards = Array.from(document.querySelectorAll(".journey-card"));
    const menu = root.querySelector("[data-filters-menu]");
    const trigger = root.querySelector("[data-filters-trigger]");
    const panel = root.querySelector("[data-filters-panel]");
    const backdrop = root.querySelector("[data-filters-backdrop]");
    const closeButton = root.querySelector("[data-filters-close]");
    const resetButton = root.querySelector("[data-filters-reset]");
    const summary = root.querySelector("[data-filters-summary]");
    const badge = root.querySelector("[data-filters-badge]");
    const timeInputs = Array.from(root.querySelectorAll("[data-filter-time]"));
    const busTypeInputs = Array.from(root.querySelectorAll("[data-filter-bus-type]"));
    const emptyMessage = document.querySelector("[data-filters-empty]");

    if (cards.length === 0 || !menu || !trigger || !panel) {
      return;
    }

    let open = false;

    function setOpen(next) {
      open = next;
      menu.classList.toggle("is-open", open);
      trigger.setAttribute("aria-expanded", open ? "true" : "false");
      panel.hidden = !open;
      if (backdrop) {
        backdrop.hidden = !open;
      }

      if (open) {
        panel.querySelector("input, button")?.focus();
      }
    }

    function apply() {
      const activeTimes = timeInputs.filter((input) => input.checked).map((input) => input.dataset.filterTime);
      const activeBusTypes = busTypeInputs
        .filter((input) => input.checked)
        .map((input) => input.dataset.filterBusType);

      let visibleCount = 0;

      cards.forEach((card) => {
        const hour = Number(card.dataset.departureHour);
        const busType = card.dataset.busType;
        const matchesTime = activeTimes.length === 0 || activeTimes.includes(bucketForHour(hour));
        const matchesBusType = activeBusTypes.length === 0 || activeBusTypes.includes(busType);
        const isVisible = matchesTime && matchesBusType;

        card.hidden = !isVisible;
        if (isVisible) {
          visibleCount++;
        }
      });

      if (emptyMessage) {
        emptyMessage.hidden = visibleCount !== 0;
      }

      const activeCount = activeTimes.length + activeBusTypes.length;

      if (resetButton) {
        resetButton.hidden = activeCount === 0;
      }

      if (badge) {
        badge.hidden = activeCount === 0;
        badge.textContent = String(activeCount);
      }

      if (summary) {
        summary.textContent = activeCount === 0 ? summary.dataset.allLabel : summary.dataset.activeLabel.replace("{0}", activeCount);
      }
    }

    if (summary) {
      summary.dataset.allLabel = summary.textContent;
    }

    trigger.addEventListener("click", () => setOpen(!open));
    closeButton?.addEventListener("click", () => {
      setOpen(false);
      trigger.focus();
    });
    backdrop?.addEventListener("click", () => setOpen(false));

    document.addEventListener("pointerdown", (event) => {
      if (open && !menu.contains(event.target)) {
        setOpen(false);
      }
    });

    document.addEventListener("keydown", (event) => {
      if (open && event.key === "Escape") {
        event.preventDefault();
        setOpen(false);
        trigger.focus();
      }
    });

    timeInputs.concat(busTypeInputs).forEach((input) => {
      input.addEventListener("change", apply);
    });

    resetButton?.addEventListener("click", () => {
      timeInputs.concat(busTypeInputs).forEach((input) => {
        input.checked = false;
      });
      apply();
    });
  }

  document.querySelectorAll("[data-journey-filters]").forEach(initJourneyFilters);
})();
