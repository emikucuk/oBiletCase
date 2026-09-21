
(function () {
  "use strict";

  const comparers = {
    DepartureAscending: (a, b) => Number(a.dataset.departureMs) - Number(b.dataset.departureMs),
    DepartureDescending: (a, b) => Number(b.dataset.departureMs) - Number(a.dataset.departureMs),
    PriceAscending: (a, b) => Number(a.dataset.price) - Number(b.dataset.price),
    PriceDescending: (a, b) => Number(b.dataset.price) - Number(a.dataset.price),
  };

  function sortJourneyList(value) {
    const list = document.querySelector(".journey-list");
    const comparer = comparers[value];
    if (!list || !comparer) {
      return;
    }

    Array.from(list.querySelectorAll(".journey-card"))
      .sort(comparer)
      .forEach((card) => list.appendChild(card));
  }

  function initSortMenu(root) {
    const trigger = root.querySelector("[data-sort-trigger]");
    const panel = root.querySelector("[data-sort-panel]");
    const hidden = root.closest(".sort-bar")?.querySelector("[data-sort-value]");
    const label = root.querySelector("[data-sort-label]");
    const options = Array.from(panel.querySelectorAll("[data-sort-option]"));

    if (!trigger || !panel || !hidden || !label || options.length === 0) {
      return;
    }

    let open = false;
    let activeIndex = Math.max(
      0,
      options.findIndex((option) => option.getAttribute("aria-selected") === "true")
    );

    function setOpen(next) {
      open = next;
      root.classList.toggle("is-open", open);
      trigger.setAttribute("aria-expanded", open ? "true" : "false");
      panel.hidden = !open;

      if (open) {
        updateActiveOption(activeIndex);
        options[activeIndex]?.focus();
      }
    }

    function updateActiveOption(index) {
      activeIndex = (index + options.length) % options.length;
      options.forEach((option, i) => {
        option.classList.toggle("is-active", i === activeIndex);
        option.tabIndex = i === activeIndex ? 0 : -1;
      });
    }

    function selectOption(option) {
      const value = option.getAttribute("data-sort-option");

      setOpen(false);
      trigger.focus();

      if (!value || value === hidden.value) {
        return;
      }

      hidden.value = value;
      label.textContent = option.querySelector("span:last-child")?.textContent ?? "";

      options.forEach((opt) => {
        const isSelected = opt === option;
        opt.classList.toggle("is-selected", isSelected);
        opt.setAttribute("aria-selected", isSelected ? "true" : "false");
      });

      sortJourneyList(value);
    }

    root.addEventListener("click", (event) => {
      if (event.target.closest("[data-sort-panel]")) {
        return;
      }

      setOpen(!open);
    });

    trigger.addEventListener("keydown", (event) => {
      if (event.key === "ArrowDown" || event.key === "Enter" || event.key === " ") {
        event.preventDefault();
        setOpen(true);
      } else if (event.key === "Escape" && open) {
        event.preventDefault();
        setOpen(false);
      }
    });

    options.forEach((option, index) => {
      option.addEventListener("click", (event) => {
        event.stopPropagation();
        selectOption(option);
      });
      option.addEventListener("keydown", (event) => {
        if (event.key === "ArrowDown") {
          event.preventDefault();
          updateActiveOption(index + 1);
          options[activeIndex].focus();
        } else if (event.key === "ArrowUp") {
          event.preventDefault();
          updateActiveOption(index - 1);
          options[activeIndex].focus();
        } else if (event.key === "Home") {
          event.preventDefault();
          updateActiveOption(0);
          options[activeIndex].focus();
        } else if (event.key === "End") {
          event.preventDefault();
          updateActiveOption(options.length - 1);
          options[activeIndex].focus();
        } else if (event.key === "Enter" || event.key === " ") {
          event.preventDefault();
          selectOption(option);
        } else if (event.key === "Escape") {
          event.preventDefault();
          setOpen(false);
          trigger.focus();
        } else if (event.key === "Tab") {
          setOpen(false);
        }
      });
    });

    document.addEventListener("pointerdown", (event) => {
      if (open && !root.contains(event.target)) {
        setOpen(false);
      }
    });
  }

  document.querySelectorAll("[data-sort-menu]").forEach(initSortMenu);
})();
