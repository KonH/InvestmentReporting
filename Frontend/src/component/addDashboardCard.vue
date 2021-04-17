<template>
	<div class="card" style="width: 8rem">
		<div class="card-body">
			<button class="btn btn-outline-primary btn-block">+</button>
		</div>
	</div>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { Prop } from 'vue-property-decorator';

@Options({
	name: 'AddDashboardCard',
})
export default class AddDashboardCard extends Vue {}
</script>
