<script setup>
    import BackBtn from './BackBtn.vue';
    const props = defineProps({
        header: String,
        subheader: String,
        backBtn: Boolean,
        url: String,
        btnText: String
    });
</script>
<template>
    <div class="flex flex-col w-full pl-16">
        <div class="flex items-center justify-between mb-0">
            <div class="flex items-center">
                <BackBtn v-if="backBtn"/>
                <h1 class="text-white text-4xl font-bold ml-8 mb-0">{{ header }}</h1>
            </div>
            <a class="text-white bg-deep-dark hover:bg-purple font-bold py-2 px-4 rounded text-2xl mr-20" v-if="btnText" :href="url">
                {{btnText}}
            </a>
        </div>
        <p :class="`text-white text-sm font-bold m-0 ml-8 mt-2 ${backBtn ? 'ml-20' : ''}`">{{ subheader }}</p>
    </div>

</template>