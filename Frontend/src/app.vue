<template>
	<nav-bar-view />
	<div class="container">
		<router-view />
	</div>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { Action } from 'vuex-class';
import NavBarView from '@/view/common/navBarView.vue';

@Options({
	name: 'App',
	components: {
		NavBarView,
	},
})
export default class App extends Vue {
	@Action('fetchState')
	fetchState!: () => void;

	@Action('fetchVirtualState')
	fetchVirtualState!: () => void;

	created() {
		this.fetchState();
		this.fetchVirtualState();
	}
}
</script>
<style>
@media (max-width: 1920px) {
	.container {
		min-width: 1600px;
	}
}
</style>
