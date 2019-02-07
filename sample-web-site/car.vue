<template>
  <li>This {{make}} is {{(new Date()).getFullYear() - year}} years old.</li>
</template>

<script>
export default {
      props: ['make','year']
    };
</script>
