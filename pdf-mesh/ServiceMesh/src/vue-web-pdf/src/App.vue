<script setup lang="ts">
import { computed, ref } from 'vue'

type Page = { page: number; text: string }
type Document = { fileName: string; pageCount: number; pages: Page[] }

const file = ref<File | null>(null)
const result = ref<Document | null>(null)
const rawError = ref<unknown>(null)
const error = ref<string | null>(null)
const loading = ref(false)

const hasResult = computed(() => result.value !== null)
const jsonText = computed(() => (result.value ? JSON.stringify(result.value, null, 2) : ''))
const showJson = ref(false)
const copied = ref(false)

async function copyJson() {
  if (!jsonText.value) return
  await navigator.clipboard.writeText(jsonText.value)
  copied.value = true
  setTimeout(() => (copied.value = false), 1500)
}

function onFileChange(e: Event) {
  const target = e.target as HTMLInputElement
  file.value = target.files?.[0] ?? null
  result.value = null
  rawError.value = null
  error.value = null
}

async function upload() {
  if (!file.value) return
  loading.value = true
  result.value = null
  rawError.value = null
  error.value = null
  try {
    const form = new FormData()
    form.append('file', file.value)
    const res = await fetch('/api/uploads/upload', { method: 'POST', body: form })
    const body = await res.text()
    const parsed = body ? JSON.parse(body) : null
    if (!res.ok) {
      error.value = `HTTP ${res.status}`
      rawError.value = parsed
      return
    }
    const raw = parsed?.processingResult?.storedDocument?.jsonContent
    result.value = typeof raw === 'string' ? JSON.parse(raw) : raw
  } catch (e) {
    error.value = e instanceof Error ? e.message : String(e)
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <main>
    <header>
      <h1>PDF Upload</h1>
      <div class="controls">
        <input type="file" accept=".pdf" @change="onFileChange" />
        <button :disabled="!file || loading" @click="upload">
          {{ loading ? 'Uploading…' : 'Upload' }}
        </button>
      </div>
    </header>

    <section v-if="error" class="error">
      <p>{{ error }}</p>
      <pre v-if="rawError">{{ JSON.stringify(rawError, null, 2) }}</pre>
    </section>

    <section v-if="hasResult && result" class="document">
      <div class="meta">
        <div>
          <h2>{{ result.fileName }}</h2>
          <span class="count">{{ result.pageCount }} page{{ result.pageCount === 1 ? '' : 's' }}</span>
        </div>
        <div class="actions">
          <button type="button" @click="showJson = !showJson">
            {{ showJson ? 'Pages view' : 'JSON view' }}
          </button>
          <button type="button" @click="copyJson">
            {{ copied ? 'Copied!' : 'Copy JSON' }}
          </button>
        </div>
      </div>

      <pre v-if="showJson" class="json">{{ jsonText }}</pre>

      <template v-else>
        <article v-for="page in result.pages" :key="page.page" class="page">
          <h3>Page {{ page.page }}</h3>
          <pre>{{ page.text }}</pre>
        </article>
      </template>
    </section>
  </main>
</template>

<style scoped>
main {
  max-width: 900px;
  margin: 2rem auto;
  padding: 0 1rem;
  font-family: system-ui, sans-serif;
  color: #222;
}
header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 1rem;
  border-bottom: 1px solid #e5e5e5;
  padding-bottom: 1rem;
  margin-bottom: 1.5rem;
}
h1 {
  margin: 0;
  font-size: 1.5rem;
}
.controls {
  display: flex;
  gap: 0.5rem;
  align-items: center;
}
.error {
  background: #fdecea;
  color: #b00020;
  border: 1px solid #f5c2c7;
  border-radius: 4px;
  padding: 0.75rem 1rem;
  margin-bottom: 1.5rem;
}
.error pre {
  background: transparent;
  color: inherit;
  padding: 0;
  margin: 0.5rem 0 0;
}
.document .meta {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
  margin-bottom: 1rem;
}
.actions {
  display: flex;
  gap: 0.5rem;
}
.json {
  margin: 0 0 1rem;
  white-space: pre-wrap;
  word-break: break-word;
  font-family: ui-monospace, SFMono-Regular, Menlo, monospace;
  font-size: 0.85rem;
  line-height: 1.4;
  background: #fafafa;
  border: 1px solid #e5e5e5;
  padding: 1rem;
  border-radius: 6px;
  max-height: 500px;
  overflow: auto;
}
.document h2 {
  margin: 0;
  font-size: 1.25rem;
  word-break: break-all;
}
.count {
  color: #666;
  font-size: 0.9rem;
}
.page {
  border: 1px solid #e5e5e5;
  border-radius: 6px;
  padding: 1rem;
  margin-bottom: 1rem;
  background: #fafafa;
}
.page h3 {
  margin: 0 0 0.5rem;
  font-size: 1rem;
  color: #555;
}
.page pre {
  margin: 0;
  white-space: pre-wrap;
  word-break: break-word;
  font-family: ui-monospace, SFMono-Regular, Menlo, monospace;
  font-size: 0.85rem;
  line-height: 1.4;
  background: #fff;
  border: 1px solid #eee;
  padding: 0.75rem;
  border-radius: 4px;
  max-height: 300px;
  overflow: auto;
}
</style>
