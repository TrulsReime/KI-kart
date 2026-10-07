// KI-kart for Agder. Små forbedringer i nettleseren. Siden virker også uten JavaScript,
// fordi alle filtre og skjema sendes til serveren (MVC-kontrollerne).
(function () {
  'use strict';
  const $ = (s, r = document) => r.querySelector(s);
  const $$ = (s, r = document) => [...r.querySelectorAll(s)];
  const html = document.documentElement;
  html.classList.add('js');

  // ---------- iframe på digiagder.no ----------
  const FRAMED = (() => { try { return window.self !== window.top; } catch (e) { return true; } })();
  if (FRAMED) {
    html.classList.add('framed');
    // Rapporter høyden slik at iframen aldri får eget rullefelt
    let last = 0;
    const send = () => {
      const h = Math.ceil(document.body.getBoundingClientRect().height);
      if (h !== last) { last = h; try { parent.postMessage({ type: 'ki-kart:height', height: h }, '*'); } catch (e) { } }
    };
    new ResizeObserver(send).observe(document.body);
    addEventListener('load', send);
    setInterval(send, 1000);
    // Ny side i iframen: be foreldresiden rulle til toppen av kartet
    if (document.referrer && new URL(document.referrer).origin === location.origin && !sessionStorage_get('kikart-scroll')) {
      try { parent.postMessage({ type: 'ki-kart:scroll-top' }, '*'); } catch (e) { }
    }
  }

  function sessionStorage_get(k) { try { return sessionStorage.getItem(k); } catch (e) { return null; } }
  function sessionStorage_set(k, v) { try { v == null ? sessionStorage.removeItem(k) : sessionStorage.setItem(k, v); } catch (e) { } }

  // ---------- melding (toast) ----------
  const toastEl = $('#toast');
  function toast(m) {
    toastEl.textContent = m;
    visToast();
  }
  function visToast() {
    toastEl.hidden = false;
    if (FRAMED) {
      const anker = (location.hash && $(location.hash)) || $('#view');
      toastEl.style.position = 'absolute';
      toastEl.style.bottom = 'auto';
      toastEl.style.top = (anker.getBoundingClientRect().top + scrollY + 8) + 'px';
    }
    clearTimeout(visToast.t);
    visToast.t = setTimeout(() => toastEl.hidden = true, 3600);
  }
  if (toastEl && !toastEl.hidden && toastEl.textContent.trim()) visToast();

  // ---------- kopier e-post ----------
  $$('[data-kopier]').forEach(b => b.addEventListener('click', () => {
    const tekst = b.dataset.kopier;
    if (navigator.clipboard && navigator.clipboard.writeText)
      navigator.clipboard.writeText(tekst).then(() => toast('E-postadressen er kopiert'), () => toast(tekst));
    else toast(tekst);
  }));

  // ---------- skjema som sendes automatisk når et filter endres ----------
  // Rulleposisjonen huskes, så siden ikke hopper til toppen etter hvert klikk.
  const lagret = sessionStorage_get('kikart-scroll');
  if (lagret) {
    const [sti, y] = lagret.split('|');
    if (sti === location.pathname) scrollTo(0, +y);
    sessionStorage_set('kikart-scroll', null);
  }
  // Tomme felt og standardverdier tas ikke med, så adressen blir kort og lesbar.
  const STANDARD = { sort: 'opp', ksort: 'alfa', status: 'alle' };
  function ryddOgSend(f) {
    [...f.elements].forEach(e => {
      if (!e.name || e.type === 'checkbox' || (e.type === 'radio' && !e.checked)) return;
      if (!e.value || STANDARD[e.name] === e.value) e.disabled = true;
    });
    f.submit();
  }
  $$('form[data-autosend]').forEach(f => {
    f.addEventListener('change', e => {
      if (e.target.matches('input[type=search], input[type=text]')) return;
      sessionStorage_set('kikart-scroll', location.pathname + '|' + Math.round(scrollY));
      ryddOgSend(f);
    });
    f.addEventListener('submit', e => { e.preventDefault(); ryddOgSend(f); });
  });
  // Tilbake i nettleseren kan vise en side fra hurtigminnet med deaktiverte felt
  addEventListener('pageshow', () => $$('form[data-autosend] [disabled]').forEach(e => e.disabled = false));

  // ---------- søkefelt med forslag ----------
  function prosjektUrl(id) {
    let tilbake = location.pathname + location.search;
    if (location.pathname.toLowerCase().startsWith('/prosjekter/detaljer'))
      tilbake = new URLSearchParams(location.search).get('tilbake') || '/Prosjekter';
    return `/Prosjekter/Detaljer/${id}?tilbake=${encodeURIComponent(tilbake)}`;
  }
  $$('[data-forslag]').forEach(box => {
    const inp = $('input', box), list = $('ul', box);
    let items = [], idx = -1, timer, seq = 0;
    const close = () => { list.hidden = true; inp.setAttribute('aria-expanded', 'false'); idx = -1; };
    const hl = () => [...list.children].forEach((li, i) => li.classList.toggle('on', i === idx));
    const go = p => { location.href = prosjektUrl(p.id); };
    const draw = async () => {
      const q = inp.value.trim();
      if (q.length < 2) { close(); return; }
      const mitt = ++seq;
      let data = [];
      try { data = await (await fetch('/Prosjekter/Forslag?q=' + encodeURIComponent(q))).json(); } catch (e) { return; }
      if (mitt !== seq) return;
      items = data; idx = -1; list.innerHTML = '';
      items.forEach((p, i) => {
        const li = document.createElement('li');
        li.setAttribute('role', 'option'); li.id = `${inp.id}-o${i}`;
        li.innerHTML = '<span class="st"></span><span class="sm"></span>';
        li.children[0].textContent = p.tittel; li.children[1].textContent = p.info;
        li.addEventListener('mousedown', e => { e.preventDefault(); go(p); });
        list.append(li);
      });
      if (!items.length) {
        const li = document.createElement('li'); li.className = 'none';
        li.textContent = 'Ingen prosjekter. Trykk Enter for å søke i alt.'; list.append(li);
      }
      list.hidden = false; inp.setAttribute('aria-expanded', 'true');
    };
    inp.addEventListener('input', () => { clearTimeout(timer); timer = setTimeout(draw, 150); });
    inp.addEventListener('keydown', e => {
      if (e.key === 'ArrowDown' && items.length) { e.preventDefault(); idx = Math.min(items.length - 1, idx + 1); hl(); }
      else if (e.key === 'ArrowUp' && items.length) { e.preventDefault(); idx = Math.max(0, idx - 1); hl(); }
      else if (e.key === 'Enter' && idx >= 0 && items[idx]) { e.preventDefault(); go(items[idx]); }
      else if (e.key === 'Escape') close();
    });
    inp.addEventListener('blur', () => setTimeout(close, 120));
    // Tømmes søkefeltet med krysset, oppdateres listen
    inp.addEventListener('search', () => { if (!inp.value && inp.form && inp.form.hasAttribute('data-autosend')) ryddOgSend(inp.form); });
  });

  // ---------- kart: tooltip ----------
  const tip = $('#tip');
  $$('.map-wrap a[data-navn]').forEach(a => {
    a.addEventListener('pointermove', e => {
      tip.innerHTML = '';
      const s = document.createElement('strong'); s.textContent = a.dataset.navn;
      const b1 = document.createElement('b'); b1.textContent = a.dataset.egne;
      const b2 = document.createElement('b'); b2.textContent = a.dataset.samarbeid;
      tip.append(s, document.createElement('br'), b1, ' egne prosjekter', document.createElement('br'), b2, ' samarbeid');
      tip.hidden = false;
      tip.style.left = Math.min(e.clientX + 14, innerWidth - tip.offsetWidth - 8) + 'px';
      tip.style.top = Math.min(e.clientY + 14, innerHeight - tip.offsetHeight - 8) + 'px';
    });
    a.addEventListener('pointerleave', () => tip.hidden = true);
  });

  // ---------- karusell med utvalgte prosjekter ----------
  $$('[data-karusell]').forEach(car => {
    const slides = $$('.slide', car), dots = $$('.dot', car), playBtn = $('[data-pause]', car), track = $('.slides', car);
    if (slides.length < 2) return;
    let idx = 0, play = true, timer = null;
    const show = n => {
      const prev = idx;
      idx = (n + slides.length) % slides.length;
      slides.forEach((s, i) => {
        const on = i === idx;
        s.classList.toggle('out', i === prev && !on);
        s.classList.toggle('on', on);
        s.setAttribute('aria-hidden', String(!on));
        $$('a,button', s).forEach(b => on ? b.removeAttribute('tabindex') : b.setAttribute('tabindex', '-1'));
      });
      dots.forEach((d, i) => d.setAttribute('aria-current', String(i === idx)));
    };
    const stop = () => { clearInterval(timer); timer = null; };
    const start = () => { stop(); if (play) timer = setInterval(() => show(idx + 1), 3200); };
    dots.forEach(d => d.addEventListener('click', () => { show(+d.dataset.til); start(); }));
    playBtn.addEventListener('click', () => {
      play = !play;
      playBtn.textContent = play ? '❚❚' : '▶';
      playBtn.setAttribute('aria-label', play ? 'Pause' : 'Spill av');
      track.setAttribute('aria-live', play ? 'off' : 'polite');
      play ? start() : stop();
    });
    start();
  });

  // ---------- meld inn: skjema i tre steg ----------
  const wrap = $('[data-stegskjema]');
  if (wrap) {
    const form = $('#meldinn', wrap);
    const steps = $$('fieldset[data-steg]', form);
    const stepper = $$('.stepper li', wrap);
    const err = $('#formerr', form);
    const back = $('[data-tilbake]', form), next = $('[data-neste]', form), submit = $('[data-send]', form);
    const bareSteg1 = $$('[data-bare-steg="1"]', wrap);
    let step = +wrap.dataset.start || 1;

    // Stikkord som merkelapper
    const kilde = $('[data-stikkord]', form);
    const tagBox = document.createElement('div'); tagBox.className = 'tagin';
    const tagInp = document.createElement('input');
    tagInp.placeholder = 'Skriv et stikkord og trykk Enter'; tagInp.setAttribute('aria-label', 'Legg til stikkord'); tagInp.id = 'tagInput';
    let tags = kilde.value.split(',').map(t => t.trim()).filter(Boolean);
    const lagre = () => { kilde.value = tags.join(', '); oppdater(); };
    const tegnTags = () => {
      tagBox.innerHTML = '';
      tags.forEach(t => {
        const c = document.createElement('span'); c.className = 'chip'; c.textContent = '#' + t;
        const x = document.createElement('button'); x.type = 'button'; x.textContent = '×'; x.setAttribute('aria-label', 'Fjern ' + t);
        x.addEventListener('click', () => { tags = tags.filter(y => y !== t); tegnTags(); lagre(); });
        c.append(x); tagBox.append(c);
      });
      tagBox.append(tagInp);
      $$('[data-legg-til]', form).forEach(b => b.hidden = tags.includes(b.dataset.leggTil));
    };
    const leggTil = v => {
      v = v.trim().toLowerCase().replace(/^#/, '').replace(/\s+/g, '-');
      if (v && !tags.includes(v)) tags.push(v);
      tagInp.value = ''; tegnTags(); lagre(); tagInp.focus();
    };
    tagInp.addEventListener('keydown', e => { if (e.key === 'Enter' || e.key === ',') { e.preventDefault(); leggTil(tagInp.value); } });
    tagInp.addEventListener('blur', () => { if (tagInp.value.trim()) leggTil(tagInp.value); });
    $$('[data-legg-til]', form).forEach(b => b.addEventListener('click', () => leggTil(b.dataset.leggTil)));
    kilde.type = 'hidden';
    kilde.parentNode.insertBefore(tagBox, kilde);
    $('label[for="Stikkord"]', form).setAttribute('for', 'tagInput');
    tegnTags();

    // Hva som mangler i et steg
    const verdi = n => { const e = form.elements[n]; return e ? (e.value || '').trim() : ''; };
    const mangler = n => {
      const m = [];
      $$('[data-pakrevd]', steps[n - 1]).forEach(e => {
        if (e.dataset.pakrevdRadio) { if (!form.querySelector(`input[name="${e.dataset.pakrevdRadio}"]:checked`)) m.push(e.dataset.pakrevd); }
        else if (e === kilde) { if (!tags.length) m.push(e.dataset.pakrevd); }
        else if (!e.value.trim()) m.push(e.dataset.pakrevd);
      });
      if (n === 2) {
        const epost = verdi('KontaktEpost');
        if (epost && !/.+@.+\..+/.test(epost)) m.push('gyldig e-post');
        const org = form.querySelector('input[name="Organisering"]:checked');
        if (org && org.value === 'Samarbeid' && $$('input[name="Deltakere"]:checked', form).length < 2) m.push('minst to kommuner');
      }
      return m;
    };

    const show = n => {
      step = n;
      steps.forEach((s, i) => s.hidden = i + 1 !== n);
      stepper.forEach((li, i) => {
        const nr = i + 1;
        li.className = nr === n ? 'on' : nr < n ? 'done' : '';
        $('.sn', li).textContent = nr < n ? '✓' : String(nr);
      });
      back.hidden = n === 1; next.hidden = n === 3; submit.hidden = n !== 3;
      bareSteg1.forEach(e => e.hidden = n !== 1);
    };
    const tilToppen = () => {
      if (FRAMED) { try { parent.postMessage({ type: 'ki-kart:scroll-top' }, '*'); } catch (e) { } }
      else wrap.scrollIntoView({ behavior: 'smooth', block: 'start' });
      const f = steps[step - 1].querySelector('input:not([type=hidden]),textarea,select');
      f && setTimeout(() => f.focus({ preventScroll: true }), 50);
    };
    next.addEventListener('click', () => {
      const m = mangler(step);
      if (m.length) { err.textContent = 'Fyll ut: ' + m.join(', ') + '.'; return; }
      err.textContent = ''; show(step + 1); tilToppen();
    });
    back.addEventListener('click', () => { err.textContent = ''; show(step - 1); tilToppen(); });
    form.addEventListener('submit', e => {
      const m = [...mangler(1), ...mangler(2)];
      if (m.length) { e.preventDefault(); err.textContent = 'Fyll ut: ' + m.join(', ') + '.'; }
    });

    // Vis og skjul felt, og oppdater forhåndsvisningen
    const FASE_CLS = { 'Idé': 'f-ide', 'Pilot': 'f-pilot', 'Drift': 'f-drift', 'Fullført': 'f-avsl', 'Avbrutt': 'f-stopp' };
    const pv = n => $(`#livePrev [data-vis="${n}"]`);
    function oppdater() {
      const org = (form.querySelector('input[name="Organisering"]:checked') || {}).value;
      const fase = (form.querySelector('input[name="Fase"]:checked') || {}).value;
      const sektor = (form.querySelector('input[name="Sektor"]:checked') || {}).value;
      $('#partBox').hidden = org !== 'Samarbeid';
      $('#stoppBox').hidden = fase !== 'Avbrutt';
      pv('tittel').textContent = verdi('Tittel') || 'Prosjektets navn';
      pv('beskrivelse').textContent = verdi('Beskrivelse') || 'Kort beskrivelse av hva løsningen gjør.';
      pv('eier').textContent = verdi('Eier') || 'Ansvarlig kommune';
      pv('sektor').textContent = sektor || 'Sektor';
      pv('fase').textContent = fase || 'Idé';
      pv('fasepill').className = 'pill ' + (FASE_CLS[fase] || 'f-ide');
      pv('samarbeid').hidden = !(org === 'Samarbeid' && $$('input[name="Deltakere"]:checked', form).length > 1);
      pv('stikkord').textContent = (tags.length ? tags : ['stikkord']).slice(0, 3).map(t => '#' + t).join('  ');
      if (err.textContent && !mangler(step).length) err.textContent = '';
    }
    form.addEventListener('input', oppdater);
    form.addEventListener('change', oppdater);
    show(step);
    oppdater();
  }
})();
