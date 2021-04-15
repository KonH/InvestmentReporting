<template>
	<div class="card" style="width: 8rem">
		<div class="card-body">
			<router-link :to="`/broker/${brokerId}/account/new`" class="btn btn-outline-primary btn-block">+</router-link>
		</div>
	</div>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { Prop } from 'vue-property-decorator';

@Options({
	name: 'AddAccountCard',
})
export default class AddAccountCard extends Vue {
	@Prop()
	brokerId!: string;
}
</script>
